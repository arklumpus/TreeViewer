/*
    TreeViewer - Cross-platform software to draw phylogenetic trees
    Copyright (C) 2023  Giorgio Bianchini, University of Bristol
 
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
using TreeViewer;
using VectSharp;
using System.Collections.Generic;
using VectSharp.MuPDFUtils;
using System.Runtime.InteropServices;
using Avalonia.Controls;
using Avalonia.Styling;
using CSharpEditor;
using System.Threading.Tasks;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Reflection;
using System.Linq;

namespace a91b732fc289b4a9184efb9ee3a89c86b
{
    public static class MyModule
    {
        public const string Name = "Custom action script";
        public const string HelpText = "Executes custom code.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.0.0");
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public const string Id = "91b732fc-289b-4a91-84ef-b9ee3a89c86b";

        public static bool IsAvailableInCommandLine { get; } = false;

        public static string GroupName { get; } = "Action";

        public static double GroupIndex { get; } = 8;

        public static List<(string, Func<double, Page>)> SubItems { get; } = new List<(string, Func<double, Page>)>();

        public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };

        public static bool TriggerInTextBox { get; } = false;

        public static string ButtonText { get; } = "Custom script";

        public static bool IsLargeButton { get; } = true;

        private static string Icon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAPnAAAD5wHDtfxxAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAABHRJREFUWIW1ll1MXEUUx/9n7l0KhSoQ0aWA1oSStYmmhBpsDQhV+2AKNcTwYtOkfj4J7HYxpYauQBXpAhtjYrLGB9PG+NDGB0Ee6kdr0G5qaLCaYBPT+FHitkD8SLst7N6Z4wP3rnfXhd2F9f80c+d/z/nNzJ0zl5gZdnm9XpdS6jiAJmauDwQCPyJLeTyeX4noOoDXR0ZGJlbzCnunu7v7fqXUNwBaABhEVJVtcq/XexcAMPPDzDzm8Xj2ZQwgpTwGoBRARNO0utHR0TPZAgwPDy9EIpFqAJfM+O+0t7draQGIiAC0mvTn/X7/z9kmtxQMBmNEdMrsVlVWVtamBejq6nICKDK74bUmt6SUisdg5q1pAYQQTqtNRHK9APYYRORcyRcHUErRepOuArNibPsKlFhtZv4zB3n/ssW7My0AMzfaYL5db3bDMC4CMMxu40o+6uzsbCSiZiI6DCAfwFQkEtkVDAZjANDSO75VCT7EjN/mrof9mzbXcIG88SoR7hWKRsYG9v70lO+TZwVjN5g/Gh/Y97kV3OPxvA2gw5zgCSL60OFwhIaGhm5YHl0I8ZVtFU5omnbISg4AitS7YDxBAO5xll+F/FsC9AYYUMQ1rb1jzxPhJC9vdfuOl98rnQq+FAOA2dlZT0VFxQIRHSaiAwAORKPRZgDnrPgJhYiI7hNCJO/XYnwcvAgSS/YxQ4/FAEgAYFB00+aaeG0vKSnZIITYAmDDSlsghBBbiOgglj+axwzD+Ky7u7swvkQSLwDcw4T9n/a3np7ob/mYCfsB7tElDk742mYFqyeZcVRp3HzW12TtOwoLC08y83PLc6MBIcQDxcXFXydM2rqM3G63l4j85la8GAgE3l+JOpeyn4L4ly+EeGi9gecnB3h+coDT+ex3wU0bTFFqe/o42Xp1W1vZADKuivOTx15h8FsktUfLmo58t5o3HBp0aTFjSpHodza8djyBRtf13602ERWmCpBCgsGDBGyErkLXLvRtW8l47ULfNl3KaSIUCqijVu44gN/vnwOwYHYfzBBASV3fAWARzPkiJqbnz725PdkUDg26tKi4COZ8JkQRVbtgrnhyHfjAbLrcbndbJgTlO3suyzxVB6JFYuRBU6cBoKyhl8oaegkAdEOOA8gH0aJyqNq7H/d9b72fABCLxQYA/GDCnHK73U9nAuGs981Ih6wD6Ip2e2Nz8vjyM7oiHbLOWe+bSZh08k9pR0fHHbquHwGwh5nbAoHAL5lArFX/AchUqc64teTZvJPN+f1ftOYVyJX09JbMFA4NunQjNqbdLtpdusdz1T72x5nRKllw80tDd7SU7+y5bB/LyRaEQ4MuXcppgKplwa2zyePLz6hal3I6HBp05RRgeebGdLzISPEMkHgZGbq2F2ax0qRxae6Lvvhlt14AoRnGFIB8MJaUQ9Wmug/ixYqxRIw85InzSC7Fa5QiUA8Dt6C0R5KLjF3Oet+M4dC3MyOiIPphluJcnQIB220K/HvmU9SGBG+u6oBKb0nt/QfGytg3Njiw5QAAAABJRU5ErkJggg==";
        private static string Icon48Base64 = "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";
        private static string Icon64Base64 = "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";

        private static string OpenIcon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAPnAAAD5wHDtfxxAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAXdJREFUWIXtl01OwlAURs+rEIjjdiTGBZC4AWEFbMOwBEf4g2xCEgfuwqkJO1ArYxMGQmFkQCq114ElQSylhYfVhDtr0ptzevvdl1aJCGmWkSp9K/AXBDL91mVFRDVR7MXs6WBI1To6u9UhYAjqKgEcoICvmjrgABmgAJC3DmM1jJ17gH1dAulnYNVGp9VIeoKFZuc3JxCancQTiJuV+VqUnf+XAf/9Fe/NwZ+MQPzEwGl2FDz7Sh0nEvBGPbzhS2JoWAkcGCLXsQW8YRdv1AUgZxbJW8WV4ePew8QdtLMCu7EEZuF5q0jOXAPuPE7cQTsbXJ4sFdAM/3D7T1kApdSFWardRG7BBuA7M/A6RKyhZriEwRcKbACuwuChAjrhbt+OhP8Q0A0fO3Yk/JvA/J6vN3abKVxE6ovgMHMU63vnNm7fBr6e3CqfLoRPBToEX0Uz9isLBCUKdW6Wao1lNxoYUg0kdNQIuMOQilleDgdQ21+zrUDaAp9DMtiRZvmk5wAAAABJRU5ErkJggg==";
        private static string OpenIcon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAAK0SURBVGhD7ZlNbxJBGMefWVo1wkHT0tRTv4E3PVgBTUw8GI89+SW8eLApWGrTq4nfwvgJPBUwPRkvRuPRUBMbXkxj5U3YnfF5dmcJCIVl2dnpGn6X2echob//7MzQBSaEgChjyDGyLALoJvIB+puYMWaP07hom/7/uAP19wc3BFhvsd502r44YhDbWk1tn8g6FFi9tH+bA3+H19ec1lycGhwermSyH2StHPx71iscg5AnrnMD6P1Cg1VKe39w+16i4krypt30Q6f2yR5xi3fXUtnLdhEChisfFEG/3zSifwpVS3v9gz2IJRQwU0+2i34HNvF4//Kz+PKWrEeIwhKaeLIFtoRU4OVki8QmnnSyLU6hf+FmG8yz78CtjuwooWjB0tZ66nktsAC8+xvMxolq8UFqAviDQJaQ2apC99e3MOWJJAPj9dx3wGxVwGxWZIWbajkO8Y0MjgnZCQ7ea0Cn+hl6Z8eyA5257sB4+XtK5B0Y8HbdlAXxw3eA8+XjshMsvNeEZrlgWb3WkmwJfL7d9hVAh3wL5XGMyRbp55Lp3JuZA5jt2pA8W74q17waedFr0cxza0CeMbabTGf36XqmALY8HpUuJJ9QuOZJvlE+5DjzfU+SX727k5el9wB65AtikjzhKQAtGT3yjf6XVePkiakBbHnctC6hyB97kycmBtAm3/UmT5wbQIt8+RD/p/IuT4wNoE0ez3sXL/LESAA98gVf8sRQABIflKcPp8TGfWXyJO3MfEN2MJAQea/yxHAAnH0Xmnn1n7DFkZlfS+d2ZekJgx6Y5fUQKpcNzfi8M++CzwP5IxzuyFoH9Dyyk0xlD5xyNgyDs6c4njpl6HzFNfDIrzxh/8BRK+TXWYw9FsCeYC/jvKSUosHh2UqGfwR4wWXPF4sfunWzCKCbRQDdRDwAwF+adJezs1IK3QAAAABJRU5ErkJggg==";
        private static string OpenIcon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAAfPAAAHzwGGUVlPAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAqJJREFUeJztm02L01AUhp+bBmZERnBs/QD7Cwq6cCXOdKnoyl/hH9DFDFaZqgO60PF/uNK1itaFCxFE6EJwJ1jGaMWpdlqb5LjQih/9SNqb3CbNswuEc973zU1ymqRKRJhnLNMCTJMFYFqAabIATAswTRaAaQGmyQIwLcA0NkCztln0lL8FnAaWNPdoKXiEpdbypypvNNeeGvXp6Y2iq/xXCpYj7vU5J9bx5fLldxH3CYXlKX8rBvMA+z0lt2PoEwqLn8s+JuRMfL2CYfPPOb9YOKa1Qcd5/efmPq3FNTD3d4EsANMCTGPH3dB5dj3qR1Ch5o40roAlgfPiy/NmbbM4buc0BtAn0NyR5gAIMndEfg3QPVeMI+zckfIVMJ4sANMCTBPTHCB43R387g7i7uL7PRAvls4D5o4vCPc9ZV86vLruqA+1a3/toPui5X9v0fv6HvG6WutqoOHmrJORrgC37eC2GzCbrx+P2J5/M7IA3G/buO3tqMrr4mwkAQwzv5AvsVgoRdEyEB2nTvdj/fe2wB7td4GkmAdQ8FJrAEkyD/iWqIq2U2CY+cVCiYX8zJlHwcUD5cpjLQEkzrxSG/mVyl3QMAkm1Hy1vz1VAEk3D1MEkAbzMGEAaTEPEwSQJvMQMoC0mYcQAaTRPAQMIK3mIUAAaTYPYwJIu3kYEcA8mIchAcyLeRgQwKiftLNoXkSqk5qHAU+Fk3bkC6tXJjYPAe4Cs2x+miPfxwZajPg2sOPU6Tj/CzCJiFSnPfJ9LISHOgrFhI9SawfLVzd0FbRUTq0LNHUVjJC3gpwrrFRu6SyqRITmk+pRL5e78+t9+qx8yrYLNEC9UCIP8nsP3ePEhZ7uJir719ickwVgWoBpsgBMCzBNFoBpAabJAjAtwDQ/AFrMm8Yipcd4AAAAAElFTkSuQmCC";

        private static string SaveIcon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAC6SURBVFhH7ZVRCoMwDEDjTjZ//dmJxiyeaD/+6s1cUqM4samoJRXyoBAobR9pkxbV+1sDwAfHJm3zKjgUwX0GDj3Ldau5Eud6juGBI3h4IjoUenLsBTSYJbQECC+hKUB02gJAVfD3etfsrQIJ6Qz1DEQF0J76xGFi66NXkJr8ryA1JmACUhk67IKnesAE94LNbz+YgasOJ6S97A0E38DyFzzarvfscY8MXEG2GTABqwKrgiwE3BhqAO4HYlJRH8eoR1QAAAAASUVORK5CYII=";
        private static string SaveIcon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAABdoAAAXaAXbk6TQAAAH6SURBVGhD7ZjNTsJAEMd3FxQT48cTGOIDiK/gyfAhePGovgAcG09QixcPXIy+gPoAahNqojHEq09BvHgkcENC110YtSCFLfRT+ks2nWma7fxndxmmOFnQYmS9U6EIHSGEVtkYS7WcxWBaJl1S2Wv+MjxnWlZPEUUyuHU9ou88yvt18AcgEHye2ROD94g46ZJaUrmPgz8AYcEfgu1nTEUQNtb6pu/hIl4zSnUT/B5cQJDYoN1uzSgiaAI4AyKCKIDzIyKoAjhcxEuQBXDiOFV8GFlczJilkIkyVMjGYnkFFEVxfNUoxcJJshzMG0psgekYGNEEmBOxnk2dXOxKT8vg2U6mqGbZZa/vTcbyGQDeWZrusI4/wJ8ZiugSwnSbhcQFCCd2WgG+wfED6TShAK8JBXhNKMBrQgFeI1qJ25hiSY9GrzU52YJ7jpA7UVc6MXTM/o5WmBvr3zVHSAB7oKCVs1fgukKqpOaZiEtwTRHaQp3P9i2Y7hFZuAFrLEICns8PmmC6huhW9c0hxqwJ48MqlgVM+yKnEDrExkb+O3i7+wjtLNebmNLfac2+ZhsJfB0IvICpt5Bxqe1g1LzhFgoCoQCvmd9fobCQ2cT8bqGwkA3BA58mKf/iDLjebdlIk1BMhXpPP8JjJ6ixKDGDd/+Ofi6xmVYv5sai9AXTvL4ZiEc2hQAAAABJRU5ErkJggg==";
        private static string SaveIcon64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAACXBIWXMAAAfPAAAHzwGGUVlPAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAAjRJREFUeJztmkFO20AUhv83TmkUFuk1coSsCUWCyBILjoLiTWrcbiz1KCwoVlgUwpojhGM0i6CUBj8WbRcVUZ2xjf+gmW8bz/ifTzO230xEVTEcX/RyCVIBBgA6sODqcyg21xdx9CnTMvd70U6QXCXhWdH9zHB80VMJ7gQIYTn4rUYRH8XZWdFlJpcgBdB9/UQENpBgBNhrKA6HAgkGwG5zaUgo4sP4Ml33k2k6CwtRGa2T4IwAYL0EpwQALyU4JwD4V0KLHYbFHwluzoC/iMrIaQHAFi6BumuLIpyfAZUFHJxe078kw2RSuoirLKDV+Tmo2kdV8lVeOkP1JaCaHiffPlTupyT70XlXRdZ+52+CHI4v/7sBsSH3EIlWD++n379+XNTQXyEHp9e779rLvd+D117ZfuoS8GbxbwF2ADZeADsAGy+AHYCNF8AOwMYLYAdg4wWwA7BxXkDJPUGZiWpkWmaaxcOHeiPZESaTTr7KB2XL4hLlsMweH5f9m/Rkbnuz12Q/Ou/u7LTvbCVYLwFRjbZt8ABwk57MVfKRbTtrAb+W7VvbNk3RMoF1NmsBTW15laHM88j5t4AXwA7AprazwaL/99VNXWeIzs8AL4AdgE1tz4Cmz/XrwvkZ4AWwA7DxAtgB2HgB7ABsfC1QRydvGS+AHYCNrwXYAdh4AewAbAyArd3nb4CFATBlp2ChwK0JAhMB+MEOQ2Bu9CkyWTy8DwLTVyCDG8thoUAm+tSffDmePQNWO6Y25WotSwAAAABJRU5ErkJggg==";

        private static string RunIcon32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAACXBIWXMAAAPnAAAD5wHDtfxxAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAAA89JREFUWIXFll9oXEUUh78zu6GWNJhUwdikWsGE5iFVURFUYiP1RUwqfciLpZqo9c1k/5UWaVObCqab7GoK4oqYklAUKj6YWNAqKiQKtoIioiAo1tDaEtFaF2r23jk+5N71Jmaz2WxSz9PMnTO/33fPzJ25oqoEIx6Pb7bWHgG2quo96XT6O0qMaDT6s4hcAA4ODg6eXCzXBDuJROIWa+0k0AY4IrKxVPN4PH49gKrerapj0Wh0+5IBXNc9DKwHsqFQ6M5UKvVBqQADAwPT2Wz2VuBrT/9oR0dHqCiAiAjQ7tF/lkwmfyrV3I9MJpMTkRNed2N9ff0dRQF6enpqgXVe9/xyzf2w1uY1VLWhKIAxptZvi4hbLkBQQ0RqC+XlAay1Uq7pIjAFtYMVqPHbqvr7Cvj+EdC7tiiAqrYEYL4o191xnC8Bx+u2FMqT7u7uFhFpFZG9wDXAmWw2e28mk8kBtO0fb7BGY6qcvXjhfLJqQ6OudS/vEeEmY2VwrO+RHx7uffcxozyI6pvjfds/9MWj0ejLwLPeC46IyPGKiorP+/v7L/s5YWPMp4EqjIRCoZhvDmDFvoKyTYAbam/8BfeSC/ICCla0sX3/2JMijOrsUnfc9cxr689kducApqamonV1ddMisldEdgG7ZmZmWoFPfP05B5GI3GyMmb9eV/Lj6BXE/B0cc8K5HOACKDJTtaExf7bX1NSsMcZsAtYUWgJjjNkkIp3MbpoHHMc5lUgkKvMlcnkKdJ8KO9871P72yUNt76iwE3Rf2KXzZO+OKaP2IVUO2JC2fty71V93Kisrj6lqFxACDhtjmqqrqyfmvLR/GUUikbiIJL2leDqdTr9eiHopEYvFGlX1e0CAsVQq1b5gBfyGquZ3vjFmSznmXjR75qjq6UJJwbvgrwDMuoXT/xvyRmxChhPb5j8PahhjskUBABuYvPRTUeQ+0FOFQOZrFwQIh8Pn/tWUyoXTlw7iOM5HqjoCuKp6rtC0PEAymbwITHvd5pIB5oEcvb3irZe2hEdd120CJosCzM6VY15zcyQS2bFsiADI0G3h4XRzqGlJALlcrg/4xoM5EYlEHi0LIgBSaI/MARgaGvrTcZz7gX5mf6m+KhugCIjM/ysuWXc4vjwB1UnEHDTFM1cpRK5DWPt/APyG0MPZqmZ9IjkWvorGM6Cvkqs4oLtfvOQ/vFoA42C7tTP14/yB1QY4jRLVroGJQgmrBTCF6nN0pUaVxT+zld6EWVSeR6oatGtwpJg5rFwFLMpxjNmjnUd+LWXiSgC8j7ExfTz17XIm/wOHO4ntmsa58AAAAABJRU5ErkJggg==";
        private static string RunIcon48Base64 = "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";
        private static string RunIcon64Base64 = "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";

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Base64, ref Icon48Base64, ref Icon64Base64, 32);
        }

        private static VectSharp.Page GetOpenIcon(double scaling)
        {
            return GetIcon(scaling, ref OpenIcon32Base64, ref OpenIcon48Base64, ref OpenIcon64Base64, 32);
        }

        private static VectSharp.Page GetSaveIcon(double scaling)
        {
            return GetIcon(scaling, ref SaveIcon32Base64, ref SaveIcon48Base64, ref SaveIcon64Base64, 32);
        }

        private static VectSharp.Page GetRunIcon(double scaling)
        {
            return GetIcon(scaling, ref RunIcon32Base64, ref RunIcon48Base64, ref RunIcon64Base64, 32);
        }

        private static VectSharp.Page GetIcon(double scaling, ref string icon32, ref string icon48, ref string icon64, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon32);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon48);
            }
            else
            {
                bytes = Convert.FromBase64String(icon64);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            VectSharp.RasterImage icon;

            try
            {
                icon = new VectSharp.MuPDFUtils.RasterImageStream(imagePtr, bytes.Length, MuPDFCore.InputFileTypes.PNG);
            }
            catch (Exception ex)
            {
                throw ex.InnerException;
            }
            finally
            {
                Marshal.FreeHGlobal(imagePtr);
            }

            VectSharp.Page pag = new VectSharp.Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        public static async void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            await Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                Window scriptWindow = await CreateWindow(window);

                await scriptWindow.ShowDialog2(window);
            });
        }

        private static async Task<Window> CreateWindow(MainWindow parent)
        {
            ChildWindow window = new ChildWindow() { Title = "Custom action script", Width = 800, Height = 450, WindowStartupLocation = WindowStartupLocation.CenterOwner };

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                window.Classes.Add("MacOSStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                window.Classes.Add("WindowsStyle");
            }

            {
                Style style = new Style(x => x.Class("WindowsStyle").Descendant().OfType<Canvas>().Class("RibbonSeparator"));
                style.Setters.Add(new Setter() { Property = Canvas.BackgroundProperty, Value = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(177, 177, 177)) });
                style.Setters.Add(new Setter() { Property = Canvas.MarginProperty, Value = new Avalonia.Thickness(0, 0, 0, 0) });
                window.Styles.Add(style);
            }

            {
                Style style = new Style(x => x.Class("MacOSStyle").Descendant().OfType<Canvas>().Class("RibbonSeparator"));
                style.Setters.Add(new Setter() { Property = Canvas.BackgroundProperty, Value = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(206, 206, 206)) });
                window.Styles.Add(style);
            }

            Grid contents = new Grid();
            contents.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            contents.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            contents.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));

            {
                Canvas separator = new Canvas() { ZIndex = 1, Height = 1 };
                separator.Classes.Add("RibbonSeparator");
                Grid.SetRow(separator, 1);
                contents.Children.Add(separator);
            }

            StringBuilder defaultSourceCode = new StringBuilder();

            defaultSourceCode.AppendLine("using System;");
            defaultSourceCode.AppendLine("using System.Threading.Tasks;");
            defaultSourceCode.AppendLine("using TreeViewer;");
            defaultSourceCode.AppendLine();
            defaultSourceCode.AppendLine("namespace a" + Guid.NewGuid().ToString("N"));
            defaultSourceCode.AppendLine("{");
            defaultSourceCode.AppendLine("\t//Do not change class name");
            defaultSourceCode.AppendLine("\tpublic static class CustomCode");
            defaultSourceCode.AppendLine("\t{");
            defaultSourceCode.AppendLine("\t\t//Do not change method signature");
            defaultSourceCode.AppendLine("\t\tpublic static async Task PerformAction(MainWindow parentWindow, Action<double> progressAction)");
            defaultSourceCode.AppendLine("\t\t{");
            defaultSourceCode.AppendLine("\t\t\t//TODO: do something");
            defaultSourceCode.AppendLine("\t\t\t//TODO: call progressAction with a value ranging from 0 to 1 to display progress");
            defaultSourceCode.AppendLine("\t\t}");
            defaultSourceCode.AppendLine("\t}");
            defaultSourceCode.AppendLine("}");

            string editorId = "CodeEditor_" + "Custom action script".CoerceValidFileName() + "_" + Guid.NewGuid().ToString("n");

            Editor editor = await Editor.Create(initialText: defaultSourceCode.ToString(), guid: editorId, additionalShortcuts: new Shortcut[] { new Shortcut("Run script", new string[][] { new string[] { "F5" } }) });
            editor.Background = window.Background;
            editor.KeyDown += (s, e) =>
            {
                if (e.Key == Avalonia.Input.Key.F5)
                {
                    runScript();
                }
            };

            Grid.SetRow(editor, 2);
            contents.Children.Add(editor);

            async void runScript()
            {
                try
                {
                    (Assembly Assembly, CSharpCompilation Compilation) result = await editor.Compile(parent.DebuggerServer.SynchronousBreak(editor), parent.DebuggerServer.AsynchronousBreak(editor));

                    if (result.Assembly != null)
                    {
                        ProgressWindow progWin = new ProgressWindow();
                        progWin.ProgressText = "Executing script...";

                        Task dialog = progWin.ShowDialog2(window);

                        Action<double> progressAction = x =>
                        {
                            _ = Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
                            {
                                if (double.IsNaN(x))
                                {
                                    progWin.IsIndeterminate = true;
                                }
                                else
                                {
                                    progWin.IsIndeterminate = false;
                                    progWin.Progress = x;
                                }
                            });
                        };

                        object[] args = new object[] { parent, progressAction };

                        try
                        {
                            Task task = (Task)TreeViewer.ModuleMetadata.GetTypeFromAssembly(result.Assembly, "CustomCode").InvokeMember("PerformAction", BindingFlags.Default | BindingFlags.InvokeMethod, null, null, args);
                            await task;

                            progWin.Close();
                            await dialog;

                            await new MessageBox("Success!", "Script execution completed", iconType: MessageBox.MessageBoxIconTypes.Tick).ShowDialog2(window);
                        }
                        catch (Exception ex)
                        {
                            progWin.Close();
                            await dialog;

                            await new MessageBox("Error!", "An error occurred while executing the script!\n" + ex.Message).ShowDialog2(window);
                        }

                    }
                    else
                    {
                        IEnumerable<Diagnostic> failures = result.Compilation.GetDiagnostics().Where(diagnostic =>
                        diagnostic.IsWarningAsError ||
                        diagnostic.Severity == DiagnosticSeverity.Error);

                        StringBuilder message = new StringBuilder();

                        foreach (Diagnostic diagnostic in failures)
                        {
                            message.AppendLine(diagnostic.Id + ": " + diagnostic.GetMessage());
                        }

                        await new MessageBox("Error!", "Compilation error!\n" + message).ShowDialog2(window);
                    }
                }
                catch (Exception ex)
                {
                    await new MessageBox("Error!", "Compilation error!\n" + ex.Message).ShowDialog2(window);
                }

            }

            async void saveScript()
            {
                SaveFileDialog dialog = new SaveFileDialog() { Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "C# source code file", Extensions = new List<string>() { "cs" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } }, Title = "Save script..." };

                string result = await dialog.ShowAsync(window);

                if (!string.IsNullOrEmpty(result))
                {
                    System.IO.File.WriteAllText(result, editor.Text);
                }
            }

            async void openScript()
            {
                OpenFileDialog dialog;

                if (!Modules.IsMac)
                {
                    dialog = new OpenFileDialog()
                    {
                        Title = "Open tree file",
                        AllowMultiple = false,
                        Filters = new List<FileDialogFilter>() { new FileDialogFilter() { Name = "C# source code file", Extensions = new List<string>() { "cs" } }, new FileDialogFilter() { Name = "All files", Extensions = new List<string>() { "*" } } }
                    };
                }
                else
                {
                    dialog = new OpenFileDialog()
                    {
                        Title = "Open tree file",
                        AllowMultiple = false
                    };
                }

                string[] result = await dialog.ShowAsync(window);

                if (result != null && result.Length == 1)
                {
                    try
                    {
                        string text = System.IO.File.ReadAllText(result[0]);
                        await editor.SetText(text);
                    }
                    catch (Exception ex)
                    {
                        await new MessageBox("Error!", "An error occurred while reading the file!\n" + ex.Message).ShowDialog2(window);
                    }
                }
            }

            RibbonTabContent ribbonTabContent = new RibbonTabContent(new List<(string, List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>)>()
            {
                ("File", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Open", new DPIAwareBox(GetOpenIcon) { Width = 32, Height = 32 }, null, new List<(string, Control, string)>(), true, 0, (Action<int>)(_ =>{ openScript(); }), "Opens a script file."),

                    ("Save", new DPIAwareBox(GetSaveIcon) { Width = 32, Height = 32 }, null, new List<(string, Control, string)>(), true, 0, (Action<int>)(_ =>{ saveScript(); }), "Saves the script to a file."),
                }),
                ("Script", new List<(string, Control, string, List<(string, Control, string)>, bool, double, Action<int>, string)>()
                {
                    ("Run", new DPIAwareBox(GetRunIcon) { Width = 32, Height = 32 }, null, new List<(string, Control, string)>(), true, 0, (Action<int>)(_ =>{ runScript(); }), "Executes the script."),
                })
            })
            {
                Height = 80,
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch
            };

            contents.Children.Add(ribbonTabContent);

            window.Content = contents;

            return window;
        }
    }
}
