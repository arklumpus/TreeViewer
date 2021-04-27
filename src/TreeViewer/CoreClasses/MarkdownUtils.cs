using MDEdit;
using System;
using System.IO;
using VectSharp;
using VectSharp.Markdown;
using VectSharp.SVG;

namespace TreeViewer
{
    public static class MarkdownUtils
    {
        private const string imageCacheId = "bb5a724e-93f7-431d-87c3-53f31d8da16e";
        private static readonly string imageCacheFolder = Path.Combine(Path.GetTempPath(), imageCacheId);

        static MarkdownUtils()
        {
            Directory.CreateDirectory(imageCacheFolder);
        }

        public static ImageRetrievalResult ImageUriResolverAsynchronous(string imageUri, string baseUriString, InstanceStateData stateData)
        {
            if (imageUri.Trim().StartsWith("circle://") || imageUri.Trim().StartsWith("ellipse://"))
            {
                try
                {
                    string[] parameters;

                    if (imageUri.Trim().StartsWith("circle://"))
                    {
                        parameters = imageUri.Trim().Substring(9).Split(',');
                    }
                    else
                    {
                        parameters = imageUri.Trim().Substring(10).Split(',');
                    }

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    if (imageUri.Trim().StartsWith("circle://"))
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);
                        height = width;
                        if (parameters.Length > 2)
                        {
                            valid = valid && double.TryParse(parameters[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            stroke = Colour.FromCSSString(parameters[3]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width) & double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height);

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }

                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0))
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        double sqrt = Math.Sqrt(width * height);

                        pag.Graphics.Translate(pag.Width * 0.5, pag.Height * 0.5);
                        pag.Graphics.Scale(width / sqrt, height / sqrt);

                        GraphicsPath circle = new GraphicsPath().Arc(0, 0, sqrt * 0.5, 0, 2 * Math.PI).Close();

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillPath(circle, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokePath(circle, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("rect://") || imageUri.Trim().StartsWith("square://"))
            {
                try
                {
                    string[] parameters;

                    if (imageUri.Trim().StartsWith("rect://"))
                    {
                        parameters = imageUri.Trim().Substring(7).Split(',');
                    }
                    else
                    {
                        parameters = imageUri.Trim().Substring(9).Split(',');
                    }

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    if (imageUri.Trim().StartsWith("square://"))
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);
                        height = width;
                        if (parameters.Length > 2)
                        {
                            valid = valid && double.TryParse(parameters[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            stroke = Colour.FromCSSString(parameters[3]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width) & double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height);

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }

                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0))
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillRectangle(strokeThickness * 0.5, strokeThickness * 0.5, width, height, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokeRectangle(strokeThickness * 0.5, strokeThickness * 0.5, width, height, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("poly://") || imageUri.Trim().StartsWith("star://"))
            {
                try
                {
                    bool isStar = imageUri.Trim().StartsWith("star://");

                    string[] parameters;

                    parameters = imageUri.Trim().Substring(7).Split(',');

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);

                    int sides = isStar ? 5 : 3;

                    if (!double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height))
                    {
                        height = width;
                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 2)
                        {
                            valid = valid && int.TryParse(parameters[2], out sides);
                        }

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            valid = valid && int.TryParse(parameters[3], out sides);
                        }

                        if (parameters.Length > 4)
                        {
                            valid = valid && double.TryParse(parameters[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        if (parameters.Length > 5)
                        {
                            stroke = Colour.FromCSSString(parameters[5]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }


                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0) && sides > 2)
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        double sqrt = Math.Sqrt(width * height);

                        pag.Graphics.Translate(pag.Width * 0.5, pag.Height * 0.5);
                        pag.Graphics.Scale(width / sqrt, height / sqrt);

                        GraphicsPath polygon = new GraphicsPath();

                        polygon.MoveTo(sqrt * 0.5, 0);

                        if (!isStar)
                        {
                            double deltaAngle = Math.PI * 2 / sides;
                            for (int i = 1; i < sides; i++)
                            {
                                polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.5, Math.Sin(deltaAngle * i) * sqrt * 0.5);
                            }
                        }
                        else
                        {
                            double deltaAngle = Math.PI / sides;
                            for (int i = 1; i < sides * 2; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.5, Math.Sin(deltaAngle * i) * sqrt * 0.5);
                                }
                                else
                                {
                                    polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.25, Math.Sin(deltaAngle * i) * sqrt * 0.25);
                                }
                            }
                        }

                        polygon.Close();

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillPath(polygon, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokePath(polygon, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("attachment://"))
            {
                try
                {
                    string attachmentName = imageUri.Trim().Substring(13);

                    if (stateData.Attachments.TryGetValue(attachmentName, out Attachment att))
                    {
                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N"));

                        string extension;

                        using (FileStream fs = File.Create(path))
                        {
                            att.WriteToStream(fs);
                            fs.Seek(0, SeekOrigin.Begin);
                            extension = GetExtensionBasedOnContent(fs);
                        }
                        
                        if (!string.IsNullOrEmpty(extension))
                        {
                            File.Move(path, path + extension);
                            path = path + extension;
                        }

                        return new ImageRetrievalResult(path, false);

                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                }
            }
            else
            {
                return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
            }
        }

        public static ImageRetrievalResult ImageUriResolverSynchronous(string imageUri, string baseUriString, InstanceStateData stateData)
        {
            if (imageUri.Trim().StartsWith("circle://") || imageUri.Trim().StartsWith("ellipse://"))
            {
                try
                {
                    string[] parameters;

                    if (imageUri.Trim().StartsWith("circle://"))
                    {
                        parameters = imageUri.Trim().Substring(9).Split(',');
                    }
                    else
                    {
                        parameters = imageUri.Trim().Substring(10).Split(',');
                    }

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    if (imageUri.Trim().StartsWith("circle://"))
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);
                        height = width;
                        if (parameters.Length > 2)
                        {
                            valid = valid && double.TryParse(parameters[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            stroke = Colour.FromCSSString(parameters[3]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width) & double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height);

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }

                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0))
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        double sqrt = Math.Sqrt(width * height);

                        pag.Graphics.Translate(pag.Width * 0.5, pag.Height * 0.5);
                        pag.Graphics.Scale(width / sqrt, height / sqrt);

                        GraphicsPath circle = new GraphicsPath().Arc(0, 0, sqrt * 0.5, 0, 2 * Math.PI).Close();

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillPath(circle, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokePath(circle, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("rect://") || imageUri.Trim().StartsWith("square://"))
            {
                try
                {
                    string[] parameters;

                    if (imageUri.Trim().StartsWith("rect://"))
                    {
                        parameters = imageUri.Trim().Substring(7).Split(',');
                    }
                    else
                    {
                        parameters = imageUri.Trim().Substring(9).Split(',');
                    }

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    if (imageUri.Trim().StartsWith("square://"))
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);
                        height = width;
                        if (parameters.Length > 2)
                        {
                            valid = valid && double.TryParse(parameters[2], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            stroke = Colour.FromCSSString(parameters[3]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width) & double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height);

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }

                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0))
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillRectangle(strokeThickness * 0.5, strokeThickness * 0.5, width, height, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokeRectangle(strokeThickness * 0.5, strokeThickness * 0.5, width, height, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("poly://") || imageUri.Trim().StartsWith("star://"))
            {
                try
                {
                    bool isStar = imageUri.Trim().StartsWith("star://");

                    string[] parameters;

                    parameters = imageUri.Trim().Substring(7).Split(',');

                    double width;
                    double height;
                    double strokeThickness = 0;

                    Colour fill = Colour.FromRgba(0, 0, 0, 0);
                    Colour stroke = Colour.FromRgba(0, 0, 0, 0);

                    bool valid = false;

                    valid = double.TryParse(parameters[0], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out width);

                    int sides = isStar ? 5 : 3;

                    if (!double.TryParse(parameters[1], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out height))
                    {
                        height = width;
                        fill = Colour.FromCSSString(parameters[1]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 2)
                        {
                            valid = valid && int.TryParse(parameters[2], out sides);
                        }

                        if (parameters.Length > 3)
                        {
                            valid = valid && double.TryParse(parameters[3], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        if (parameters.Length > 4)
                        {
                            stroke = Colour.FromCSSString(parameters[4]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }
                    else
                    {
                        fill = Colour.FromCSSString(parameters[2]) ?? Colour.FromRgba(0, 0, 0, 0);

                        if (parameters.Length > 3)
                        {
                            valid = valid && int.TryParse(parameters[3], out sides);
                        }

                        if (parameters.Length > 4)
                        {
                            valid = valid && double.TryParse(parameters[4], System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out strokeThickness);
                        }

                        if (parameters.Length > 5)
                        {
                            stroke = Colour.FromCSSString(parameters[5]) ?? Colour.FromRgba(0, 0, 0, 0);
                        }
                    }


                    if (valid && width > 0 && height > 0 && (fill.A > 0 || stroke.A > 0) && sides > 2)
                    {
                        VectSharp.Page pag = new Page(width + strokeThickness, height + strokeThickness);

                        double sqrt = Math.Sqrt(width * height);

                        pag.Graphics.Translate(pag.Width * 0.5, pag.Height * 0.5);
                        pag.Graphics.Scale(width / sqrt, height / sqrt);

                        GraphicsPath polygon = new GraphicsPath();

                        polygon.MoveTo(sqrt * 0.5, 0);

                        if (!isStar)
                        {
                            double deltaAngle = Math.PI * 2 / sides;
                            for (int i = 1; i < sides; i++)
                            {
                                polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.5, Math.Sin(deltaAngle * i) * sqrt * 0.5);
                            }
                        }
                        else
                        {
                            double deltaAngle = Math.PI / sides;
                            for (int i = 1; i < sides * 2; i++)
                            {
                                if (i % 2 == 0)
                                {
                                    polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.5, Math.Sin(deltaAngle * i) * sqrt * 0.5);
                                }
                                else
                                {
                                    polygon.LineTo(Math.Cos(deltaAngle * i) * sqrt * 0.25, Math.Sin(deltaAngle * i) * sqrt * 0.25);
                                }
                            }
                        }

                        polygon.Close();

                        if (fill.A > 0)
                        {
                            pag.Graphics.FillPath(polygon, fill);
                        }

                        if (stroke.A > 0)
                        {
                            pag.Graphics.StrokePath(polygon, stroke, strokeThickness);
                        }

                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N") + ".svg");

                        pag.SaveAsSVG(path);

                        return new ImageRetrievalResult(path, false);
                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
                }
            }
            else if (imageUri.Trim().StartsWith("attachment://"))
            {
                try
                {
                    string attachmentName = imageUri.Trim().Substring(13);

                    if (stateData.Attachments.TryGetValue(attachmentName, out Attachment att))
                    {
                        string path = Path.Combine(imageCacheFolder, System.Guid.NewGuid().ToString("N"));

                        string extension;

                        using (FileStream fs = File.Create(path))
                        {
                            att.WriteToStream(fs);
                            fs.Seek(0, SeekOrigin.Begin);
                            extension = GetExtensionBasedOnContent(fs);
                        }

                        if (!string.IsNullOrEmpty(extension))
                        {
                            File.Move(path, path + extension);
                            path = path + extension;
                        }

                        return new ImageRetrievalResult(path, false);

                    }
                    else
                    {
                        return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                    }
                }
                catch
                {
                    return AsynchronousImageCache.ImageUriResolverAsynchronous(imageUri, baseUriString);
                }
            }
            else
            {
                return AsynchronousImageCache.ImageUriResolverSynchronous(imageUri, baseUriString);
            }
        }

        public static MarkdownRenderer GetMarkdownRenderer(InstanceStateData stateData)
        {
            MarkdownRenderer tbr = new MarkdownRenderer();

            tbr.ImageUriResolver = (a, b) => MarkdownUtils.ImageUriResolverSynchronous(a, b, stateData);
            tbr.RasterImageLoader = imageFile => new VectSharp.MuPDFUtils.RasterImageFile(imageFile);

            return tbr;
        }

        private static string GetExtensionBasedOnContent(Stream fileStream)
        {
            bool isSvg = false;

            try
            {
                using (var xmlReader = System.Xml.XmlReader.Create(fileStream))
                {
                    isSvg = xmlReader.MoveToContent() == System.Xml.XmlNodeType.Element && "svg".Equals(xmlReader.Name, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                isSvg = false;
            }

            if (isSvg)
            {
                return ".svg";
            }
            else
            {
                fileStream.Seek(0, SeekOrigin.Begin);
                byte[] header = new byte[8];

                for (int i = 0; i < header.Length; i++)
                {
                    header[i] = (byte)fileStream.ReadByte();
                }

                if (header[0] == 0x42 && header[1] == 0x4D)
                {
                    return ".bmp";
                }
                else if (header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38)
                {
                    return ".gif";
                }
                else if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF && (header[3] == 0xDB || header[3] == 0xE0 || header[3] == 0xEE || header[3] == 0xE1))
                {
                    return ".jpg";
                }
                else if (header[0] == 0x25 && header[1] == 0x50 && header[2] == 0x44 && header[3] == 0x46 && header[4] == 0x2D)
                {
                    return ".pdf";
                }
                else if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47 && header[4] == 0x0D && header[5] == 0x0A && header[6] == 0x1A && header[7] == 0x0A)
                {
                    return ".png";
                }
                else if ((header[0] == 0x49 && header[1] == 0x49 && header[2] == 0x2A && header[3] == 0x00) || (header[0] == 0x4D && header[1] == 0x4D && header[2] == 0x00 && header[3] == 0x2A))
                {
                    return ".tif";
                }
                else
                {
                    return null;
                }
            }
        }
    }
}
