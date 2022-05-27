/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2021  Giorgio Bianchini
 
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published by
    the Free Software Foundation, version 3.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace TreeViewer
{
    public class FileCopyException : Exception
    {
        public FileCopyException(string message) : base(message)
        {

        }
    }

    public static class ModuleUtils
    {
        public static void CopyFiles(IEnumerable<(string origin, string destination)> filesToCopy)
        {
            foreach ((string origin, string destination) file in filesToCopy)
            {
                if (file.destination != "-")
                {
                    File.Copy(file.origin, file.destination, true);
                }
                else
                {
                    if (File.Exists(file.origin))
                    {
                        File.Delete(file.destination);
                    }
                }
            }
        }

        public static List<List<(string, Dictionary<string, object>)>> DeserializeModules(string serializedModules, List<Attachment> attachments, Func<RSAParameters?, bool> askForCodePermission)
        {
            List<List<(string, Dictionary<string, object>)>> tbr = new List<List<(string, Dictionary<string, object>)>>();

            List<List<string[]>> allModules = System.Text.Json.JsonSerializer.Deserialize<List<List<string[]>>>(serializedModules, Modules.DefaultSerializationOptions);

            int startIndex = 0;

            RSAParameters? publicKey = null;

            if (allModules.Count > 0 && allModules[0].Count > 0 && allModules[0][0].Length > 0 && allModules[0][0][0] == CryptoUtils.FileSignatureGuid)
            {
                string signature = allModules[0][0][1];
                string publicKeySerialized = allModules[0][0][2];

                CryptoUtils.PublicKeyHolder publicKeyHolder = JsonSerializer.Deserialize<CryptoUtils.PublicKeyHolder>(publicKeySerialized, Modules.DefaultSerializationOptions);
                publicKey = new RSAParameters()
                {
                    Exponent = Convert.FromBase64String(publicKeyHolder.Exponent),
                    Modulus = Convert.FromBase64String(publicKeyHolder.Modulus)
                };

                string signedModules = JsonSerializer.Serialize<List<List<string[]>>>(allModules.Skip(1).ToList(), Modules.DefaultSerializationOptions);

                if (CryptoUtils.VerifyStringSignature(signedModules, signature, CryptoUtils.FileRSADecrypters))
                {
                    askForCodePermission = k => true;
                }

                startIndex++;
            }

            for (int i = startIndex; i < allModules.Count; i++)
            {
                List<(string, Dictionary<string, object>)> currModules = new List<(string, Dictionary<string, object>)>();
                for (int j = 0; j < allModules[i].Count; j++)
                {
                    string moduleId = allModules[i][j][0];
                    Dictionary<string, object> parameters = DeserializeParameters(allModules[i][j][1], attachments, () => askForCodePermission(publicKey));
                    currModules.Add((moduleId, parameters));
                }

                tbr.Add(currModules);
            }

            return tbr;
        }

        private static Dictionary<string, object> DeserializeParameters(string serializedParameters, List<Attachment> attachments, Func<bool> askForCodePermission)
        {
            Dictionary<string, object> tbr = new Dictionary<string, object>();

            List<string[]> allParameters = System.Text.Json.JsonSerializer.Deserialize<List<string[]>>(serializedParameters, Modules.DefaultSerializationOptions);

            foreach (string[] parameter in allParameters)
            {
                string key = parameter[0];

                object value = null;

                if (parameter[1] == "bool")
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<bool>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "int")
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<int>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "string")
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<string>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "string[]")
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<string[]>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "double")
                {
                    value = System.Text.Json.JsonSerializer.Deserialize<double>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "font")
                {
                    string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(parameter[2], Modules.DefaultSerializationOptions);

                    string familyName = System.Text.Json.JsonSerializer.Deserialize<string>(items[0]);

                    if (VectSharp.FontFamily.StandardFamilies.Contains(familyName))
                    {
                        value = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(familyName), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                    }
                    else if (familyName.StartsWith("attachment://"))
                    {
                        string attachmentName = familyName.Substring(13);

                        bool assigned = false;

                        for (int i = 0; i < attachments.Count; i++)
                        {
                            if (attachments[i].Name == attachmentName)
                            {
                                AttachmentFontFamily family = attachments[i].GetFontFamily();

                                if (family != null)
                                {
                                    assigned = true;
                                    value = new VectSharp.Font(family, System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                                }
                                break;
                            }
                        }
                        
                        if (!assigned)
                        {
                            value = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                        }
                    }
                    else if (familyName.StartsWith("webfont://"))
                    {
                        try
                        {
                            string fontName = familyName.Substring(10);
                            fontName = fontName.Substring(0, fontName.LastIndexOf("["));

                            string style = familyName.Substring(familyName.LastIndexOf("[") + 1);
                            style = style.Substring(0, style.Length - 1);

                            FontChoiceWindow.WebFont webFont = null;

                            for (int i = 0; i < FontChoiceWindow.WebFonts.Count; i++)
                            {
                                if (FontChoiceWindow.WebFonts[i].Name == fontName)
                                {
                                    webFont = FontChoiceWindow.WebFonts[i];
                                    break;
                                }
                            }

                            if (webFont != null)
                            {
                                value = new VectSharp.Font(WebFontFamily.Create(webFont, style), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                            }
                            else
                            {
                                value = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                            }
                        }
                        catch
                        {
                            value = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                        }
                    }
                    else
                    {
                        value = new VectSharp.Font(VectSharp.FontFamily.ResolveFontFamily(VectSharp.FontFamily.StandardFontFamilies.Helvetica), System.Text.Json.JsonSerializer.Deserialize<double>(items[1], Modules.DefaultSerializationOptions));
                    }
                }
                else if (parameter[1] == "point")
                {
                    double[] items = System.Text.Json.JsonSerializer.Deserialize<double[]>(parameter[2], Modules.DefaultSerializationOptions);
                    value = new VectSharp.Point(items[0], items[1]);
                }
                else if (parameter[1] == "colour")
                {
                    double[] items = System.Text.Json.JsonSerializer.Deserialize<double[]>(parameter[2], Modules.DefaultSerializationOptions);
                    value = VectSharp.Colour.FromRgba(items[0], items[1], items[2], items[3]);
                }
                else if (parameter[1] == "dash")
                {
                    double[] items = System.Text.Json.JsonSerializer.Deserialize<double[]>(parameter[2], Modules.DefaultSerializationOptions);
                    value = new VectSharp.LineDash(items[0], items[1], items[2]);
                }
                else if (parameter[1] == "formatterOptions")
                {
                    List<object> parameters = DeserializeList(parameter[2]);
                    if (parameters.Count < 7)
                    {
                        if (askForCodePermission())
                        {
                            value = new FormatterOptions((string)parameters[0]) { Parameters = parameters.ToArray() };
                        }
                        else
                        {
                            parameters[0] = Modules.DefaultAttributeConverters[0];
                            parameters[1] = true;
                            value = new FormatterOptions((string)parameters[0]) { Parameters = parameters.ToArray() };
                        }

                    }
                    else
                    {
                        if (askForCodePermission())
                        {
                            value = new FormatterOptions((string)parameters[6]) { Parameters = parameters.ToArray() };
                        }
                        else
                        {
                            parameters[6] = Modules.DefaultAttributeConverters[1];
                            parameters[7] = true;
                            value = new FormatterOptions((string)parameters[6]) { Parameters = parameters.ToArray() };
                        }
                    }
                }
                else if (parameter[1] == "numberFormatterOptions")
                {
                    string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(parameter[2], Modules.DefaultSerializationOptions);

                    List<object> parameters = DeserializeList(items[3]);

                    if (askForCodePermission())
                    {
                        value = new NumberFormatterOptions((string)parameters[0]) { AttributeName = System.Text.Json.JsonSerializer.Deserialize<string>(items[0]), AttributeType = System.Text.Json.JsonSerializer.Deserialize<string>(items[1]), DefaultValue = System.Text.Json.JsonSerializer.Deserialize<double>(items[2]), Parameters = parameters.ToArray() };
                    }
                    else
                    {
                        string attrType = System.Text.Json.JsonSerializer.Deserialize<string>(items[1], Modules.DefaultSerializationOptions);
                        parameters[0] = attrType.Equals("String", StringComparison.OrdinalIgnoreCase) ? Modules.DefaultAttributeConvertersToDouble[0] : Modules.DefaultAttributeConvertersToDouble[1];
                        parameters[^1] = true;

                        value = new NumberFormatterOptions(attrType.Equals("String", StringComparison.OrdinalIgnoreCase) ? Modules.DefaultAttributeConvertersToDouble[0] : Modules.DefaultAttributeConvertersToDouble[1]) { AttributeName = System.Text.Json.JsonSerializer.Deserialize<string>(items[0]), AttributeType = System.Text.Json.JsonSerializer.Deserialize<string>(items[1]), DefaultValue = System.Text.Json.JsonSerializer.Deserialize<double>(items[2]), Parameters = parameters.ToArray() };
                    }

                }
                else if (parameter[1] == "colourFormatterOptions")
                {
                    string[] items = System.Text.Json.JsonSerializer.Deserialize<string[]>(parameter[2], Modules.DefaultSerializationOptions);

                    List<object> parameters = DeserializeList(items[3]);

                    double[] colour = System.Text.Json.JsonSerializer.Deserialize<double[]>(items[2], Modules.DefaultSerializationOptions);

                    if (askForCodePermission())
                    {
                        value = new ColourFormatterOptions((string)parameters[0], parameters.ToArray()) { AttributeName = System.Text.Json.JsonSerializer.Deserialize<string>(items[0]), AttributeType = System.Text.Json.JsonSerializer.Deserialize<string>(items[1]), DefaultColour = VectSharp.Colour.FromRgba(colour[0], colour[1], colour[2], colour[3]) };
                    }
                    else
                    {
                        string attrType = System.Text.Json.JsonSerializer.Deserialize<string>(items[1], Modules.DefaultSerializationOptions);
                        parameters[0] = attrType.Equals("String", StringComparison.OrdinalIgnoreCase) ? Modules.DefaultAttributeConvertersToColour[0] : Modules.DefaultAttributeConvertersToColour[1];
                        parameters[^1] = true;

                        value = new ColourFormatterOptions(attrType.Equals("String", StringComparison.OrdinalIgnoreCase) ? Modules.DefaultAttributeConvertersToColourCompiled[0].Formatter : Modules.DefaultAttributeConvertersToColourCompiled[1].Formatter) { AttributeName = System.Text.Json.JsonSerializer.Deserialize<string>(items[0]), AttributeType = attrType, DefaultColour = VectSharp.Colour.FromRgba(colour[0], colour[1], colour[2], colour[3]), Parameters = parameters.ToArray() };
                    }
                }
                else if (parameter[1] == "compiledCode")
                {
                    if (askForCodePermission())
                    {
                        value = new CompiledCode(System.Text.Json.JsonSerializer.Deserialize<string>(parameter[2], Modules.DefaultSerializationOptions), true);
                    }
                    else
                    {
                        value = new CompiledCode(CompiledCode.EmptyCode, true);
                    }
                }
                else if (parameter[1] == "attachment")
                {
                    value = "b0a73d1a-ff8a-4512-a481-9bccbba629bd://" + System.Text.Json.JsonSerializer.Deserialize<string>(parameter[2], Modules.DefaultSerializationOptions);
                }
                else if (parameter[1] == "null")
                {
                    value = null;
                }

                tbr.Add(key, value);
            }

            return tbr;
        }

        private static List<object> DeserializeList(string serializedList)
        {
            List<string[]> allObjects = System.Text.Json.JsonSerializer.Deserialize<List<string[]>>(serializedList, Modules.DefaultSerializationOptions);

            List<object> tbr = new List<object>();

            foreach (string[] currObject in allObjects)
            {
                if (currObject[0] == "bool")
                {
                    tbr.Add(System.Text.Json.JsonSerializer.Deserialize<bool>(currObject[1], Modules.DefaultSerializationOptions));
                }
                else if (currObject[0] == "int")
                {
                    tbr.Add(System.Text.Json.JsonSerializer.Deserialize<int>(currObject[1], Modules.DefaultSerializationOptions));
                }
                if (currObject[0] == "string")
                {
                    tbr.Add(System.Text.Json.JsonSerializer.Deserialize<string>(currObject[1], Modules.DefaultSerializationOptions));
                }
                if (currObject[0] == "double")
                {
                    try
                    {
                        if (currObject[1] != "Infinity")
                        {
                            tbr.Add(System.Text.Json.JsonSerializer.Deserialize<double>(currObject[1], Modules.DefaultSerializationOptions));
                        }
                        else
                        {
                            tbr.Add(double.Parse(currObject[1]));
                        }
                    }
                    catch
                    {
                        tbr.Add(double.Parse(currObject[1]));
                    }
                }
                else if (currObject[0] == "gradient")
                {
                    tbr.Add(Gradient.DeserializeJson(currObject[1]));
                }
            }

            return tbr;
        }

    }
}
