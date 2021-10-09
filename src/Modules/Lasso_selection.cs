using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using PhyloTree;
using TreeViewer;
using VectSharp;
using VectSharp.Canvas;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace LassoSelection
{
    /// <summary>
    /// This module can be used to perform a "Lasso selection", i.e. to select nodes based on their position in the plot.
    /// 
    /// When you enable this module, a message is shown indicating that lasso selection is active. You can then use the mouse
    /// to draw a polygon on the tree plot (every time you click, a new point is added). When you reach the last vertex of the
    /// polygon, double click to close the shape.
    /// 
    /// At this point, a new window is shown, which lets you choose one of the attributes that are present on the selected tips.
    /// When you click on `OK`, the values of the selected attribute for the nodes that fall within the selected area are copied
    /// to the clipboard and can be pasted into other software (e.g. a text editor).
    /// </summary>
    public static class MyModule
    {
        public const string Name = "Lasso selection";
        public const string HelpText = "Selects tips from the tree.";
        public const string Author = "Giorgio Bianchini";
        public static Version Version = new Version("1.1.1");
        public const string Id = "a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6";
        public const ModuleTypes ModuleType = ModuleTypes.Action;

        public static bool IsAvailableInCommandLine { get; } = false;
        public static string ButtonText { get; } = "Lasso selection";
		public static List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)> ShortcutKeys { get; } = new List<(Avalonia.Input.Key, Avalonia.Input.KeyModifiers)>() { (Avalonia.Input.Key.None, Avalonia.Input.KeyModifiers.None) };
        public static bool TriggerInTextBox { get; } = false;

        public static string GroupName { get; } = "Selection";

        public static double GroupIndex { get; } = 7;

        public static bool IsLargeButton { get; } = true;

        public static List<(string, Func<double, VectSharp.Page>)> SubItems { get; } = new List<(string, Func<double, VectSharp.Page>)>();

        private static string Icon32Base64 = "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";
        private static string Icon48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAACXBIWXMAABYlAAAWJQFJUiTwAAAAGXRFWHRTb2Z0d2FyZQB3d3cuaW5rc2NhcGUub3Jnm+48GgAACn1JREFUaIG1mXtwXHUVxz/n3t0kNNm0WrDt0KAUQYSCaIRBHKWd0VZSoAVmQQHFTiFobabcu4GWQuBCCpTUvbuliDbIQBFkJBb6IO1QnaECjoJFwAFfIxUNlZcVkzQl7e79Hf/Yu8lmu5vm5fln9/we53zPPed3zvndK4yRHMeZB8zt6+trbW9vPzBWOeOlyFg3ikgb8JlYLJYFWvLjzc3N1UEQXCUi23zf75oIkMORNdaNIrIFQFWXNTU11ebHjTH3isgPgNQE4DsijdmA/v7+e4D9wJSKioqlAIlEYj7wbQAR2TkRAI9EMp7NruuuBZqB9zKZzOxoNPpbYBbwK9/35wI6ARiHpTGfAQDbtpNBECwDPhaNRn9DDvwBVb2aEuA9z7N6e3vPicViuz3P6x+P7jyNOYQA1q5d+w7wQMieEP7emkql/lZqfXd39w9V9bmenp70ePQW0rg8ABAEQZtt241AFHixq6ur5OF1HOd6EWkM2b8Wz3ueZ71oPneFombH7QsfHan+cXkAYN26df8EXg3Zjo6OjqB4TSKRuFhE1oTsT33fH2LkBd62SS8En/05qg+L8kjDzVuWj1T/uA0IaV/4+9HiiUQiUa+qD4e6nq+trV1CwfmYv2rTjMCYXwlcNLBJuG2e98THRqJ4ogz4D4CqTi0cdBznVFV9CqgG/iEiFxUe3oaWzafZduS3KJ8vkjc5krXvHIniCTWAAg84jnOuiDwPTA+H6lT1rsbGxijAgls2zwd5HjgunM8qtA9IFBY33LSl2LDDaEINEJGpIfivi8jTwBRVfQt4DTgE/LK9vT3T0LL5alXZBtTmsLJfhEU7Whdeq7A1j00tvQd02Fo1JAs1NTVVRqPRF4BsJBKZ29bW1jsS9CKyT1VR1amu6zYDbTlcvCYiDV1dXf+aOXPm8amU/0bDLed6KLcWbN9r4IIdty98GcCybUeDYB5QJcgXFrRsvaKzlUfK6R7iAdu2jwJOAuqz2exoepm8B2YDawFR1V3GmC/5vt/V0dERvDz5wrcabt72WBH4V9XOnr2jNQceoNM7fw/IQJ1QWHuet72WMjTEgHQ6/V9gVcguSSQSi46EvLm5udoY8/EieY9ls9mvhfJo8DqnTwp6nkX0sgJgT9q2dc7J3c9XuK475MAesPvvAPaG7HQJsjeW018qviSRSOxU1a8A79u2fXpYcQfIdd2TgAbgPODLQNXAZpFkMpm8njBVLvCemq1B0MngYUWE5JnWyzf09vbWq2onUNvV1VVdWEMWtGy5UuEnIXtQ1Zy6Y/VFbxSDLVWJ1RjzbRH5A3BMEAQPNDY2xmOx2Lmqmgd9QtGebF6WMWYjBXleg2BTAfgsqss6Wxdt6Mw9iDRwDPCz4gLY2Xrhow23bP0uyjlAJWL5wMJisCWzUCqV2quq3w3Zhpqamg9UdTuwrAD8XuDHInJJJpOZCvQBWJZVbNzAQ1I0vX31og0AjuOcBpwTTm04HIUoAcsBAyBwYS71jsAAgL6+vieB/I2qgtxTflZEbgTO8H1/pu/71ySTySfWr1/fA+wBUNVZQ2EMHkhBvjHHe6YKoLAv8n1/VykM2+9YuBvlwYEHoJKub9wQLVxTtpmrqalZDdSRewLfMcZ05A9lGdoDnEaupR4g0x1plymZG4CZwLFHBT1XA/cC54ZL7meYe0M2EqyKBPYlwBTg5GnTpjcBfn6+pAccx/kauYsKInKn7/v3HwE8qron/Ht84fiO9Q0HRaUtzwusijuPH6Wq94VDN7iu661YsWJyKbk7vYvfA1oLhrz5qzbNyDOHecBxnGNFJN98PReLxW4bDngB7QEQkROampqOqaio+AowH/jFH4d6Ycb+msolk+3J7T09PTeSO+C3ZjKZ77mum45Go/fefffd3YWCD9i190zKdi8mV2dithVpBa6GIg/E43FbRB4FjhGRfcAVnudlR4Lesqw3AFT1pGg0+o6q/lRVr1LV1XUHN5tiL7zefYptjDmbXDj0AUcDqzOZzJuO41xVKHuXNzcr4DAoYPH5Nz151mEG1NXVrSIXm2qMWTzK1yL/GMSHBfSpaqeqJqdMmVJluiPtwFvhmhn7ayqXpNPpt33fT2QymeOBu4AeYIplWdcWC+9cveiXhX1SYEkaVIYUMsdxnhGROaqaSqVS7ijA4zjOLBHJF5pLM5nM1vXr1x8sXLPg5q1NKnpPyL5dvf/gCR2pSz/Mz69cufIjBw8evMyyrGeTyeQfi3Us8J6apUHwOmHhFPjmkDNgjLkqEol8saur6/HRgAewLCtQ1fz/3cXgAfoisfsnBT0rgGOBGQdiVZczeKdmzZo1HwA/Kqej0zt/T0PL1jToSgCF1iEGhNfDf44WPICqmvx/ESmZ3XZ5c/sXtGxuVyRMDPrp0eo5YPffMSmoXBmyn5io+8AQAximQBp4c3AP00arZ5d36f5CfsIMiEQiA72MMcYut84S3i1gR23AYfLGKyBP/f39IzJA1C7sbKeXWwewfPnyafkraDmaMAOMMSMLIdsakQcSicRXbdveG4vFNg2nd8IMiEajAx5Q1bIeqH69730gv/boOd4zh3UD8XjcVtUUYGs+tZWhCTMgCIIReaCj49KAwfdI1iQOHF285rjjjlsCnErOscO2Mv8XA8ql0QIaCCMrMEPCaOnSpTWq6oXsw+l0+vfDCRrVu9HGxsZodXX1cyJSq6orUqnUtvxcZWXlQAjZtl02hEJ6l1zrTVCUSquqqhLADODDSCTSUmLv2A2IRCKVInIqUCMiW13X3WmMuS6dTv9p8uTJQU9PDzB8FgppMBNJMN3zvKre3t5rVXU2cHk4k25ra3ur5O4CGlUI3XffffuNMWcB+a8v8yzLetV13XX79u0b6OdLhZDneVXXXXfdGbn5wRASlWkAqpom1yJPCmWct3z58k8dCdOYv9A4jnOBiHyf3Huk/MutqeH/bwG9xpjZInI6uXD5JBBR1Ul/qZ6zDKEtNCbZefvCZtd1d6vq30XkA2AxuejoU9WmVCr1YKHuhpYtA5lpXJ+YGhsbo7FYbKmq3gaUvFEVkzHm+L/E5nxZkI3h0CPbWxd+s3BNIpE4U1UfY/AFwqaKioprwmZviAHjykLt7e2ZZDK5LhKJnAL8u2i6H/g98BCQEJF5QRBMT6fTbxa1E4dV42Qy+TtjzOdV9efh0CWHDh3anQ/BQhr3FxrP8yLhJ6OjAVR1jWVZG2Ox2N/K3eZE7XeUgaxbshqHd/C467rXkvtkO8uyrEXAKxNmQAj+ESCeH0qlUke8QxvbepfBsnFiQ8uWjaAvKfJS8GHVKzu/P78vP+n7/obm5uZfG2NuFJF/Fcsa8xmIx+N2XV3dQ8CV4dDdvu+vHGZLwd7H7b6TK/sp/QADgT8rvFTOqHEf4ng8bs+cOfNBEckfvjbf91eMRkbDLVvuRLm+jBHFNMQokHX5iTEZ4LruRuBbIXuH7/s3j0VO3Hu8oieoOtHC1AtWPWg9cCa5N4EjolEbUHR5v8v3/VXDbhglzWt+uto+qv8MQetB6gXqFU4GSlX3Q2PxgCQSieuNMT2pVKrsBXwiqYxRn0Tw/wfNXD/Y7KGQlgAAAABJRU5ErkJggg==";
        private static string Icon64Base64 = "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";

        private static string IconCopy32Base64 = "iVBORw0KGgoAAAANSUhEUgAAACAAAAAgCAYAAABzenr0AAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFaSURBVFhHxZZNasMwEIXlkrOEHqNk2UAO0G59AnudLrq3yQG8TRfdteBVoWSRIwQTeohcIZHMyCjWjzWSxv1gkBbGeRq9eXHGOEVRXMUaS13X/fvQCAGxhB7iAdZ/w0vA/vfM1m/fxorFS8DH4Q92REx54Hn71ZeLZB4wtZsSTQB5u0dYPdC+b4aixMuEErgOETZaVVUlVjQLWJ28PC3vrqYsS6PhuBFh58eQnOoUeDg+aXKiroACEgGY5CQRgBll0itolVGWNSZawO7zONlmF1YB45faXvxzusAuDE2AmPkQXG12oQXR6+qxrxSYujYWSGJCTBeHDvB4hZ0bnvmws5Oyi4ynplbI6DbiHcVZlmmVEhIPYAg6jmif9IJ0um38XN4Sf8ezdCDPc60kyQSoiSnLRtM0w8dItABscqo/Hgzmi0g823VdX3L0VGabAtvJg6cAtt6Y287YDaOfAyHHYnfQAAAAAElFTkSuQmCC";
        private static string IconCopy48Base64 = "iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAYAAABXAvmHAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAFiQAABYkAZsVxhQAAAHOSURBVGhD7Zg7TsQwEIYdRJ1bUHAJKiqQ2I6WJidIalCy1MkJtqGlQAIBFRUSVwgF4ia8MpFjeY3txI7t2OBPGq290mpnPPNPJk4QJs/zb7y0StM05D+NAgHYxsYh7eDPYAk+gC0N1HWNd3Kunl7R9fMb3sl5WJ/gFUJFURjXgFYGpjrvguBLiKDShY7Ob4mpELsQB2EAINTjizuu+YQwAJ+EKuPvlhAN9HLafOJ/ZEAGLe5kBBuTqHCUoLsNWzaiTvR4uUps9Hoew2Hs9jtFTg/2pF1q6kylC8xUA1oldHa4/0vYbJZcEUU8B9nTnjURiwZg4mkfS8gUvKZAm4iYgTFAqKvqXkmYKlgP4OblHX18fuGdeSYFwDs92mTYdB5wqoGpwlRBGADMOyEgHOZg3gFzxVgpijLmtIRssGgAJsp064UGL7WBlwyVFyNdbNyxEiAAGt1bPBn0YUcNLE0MYGmMK3luF6JvHGQMXcjLDGRZJjWa4Epos9nAR9VvOpyW0BhQYlBC7CkPDM535VP2X3QEkwGe84D1AEzMOyLnrcCOEqrA79u2JQb7zoSOe11C1MkT0bJ4GwDjvHBSttKF8HIuVZqm67IsJbcCCP0ADHhS2AQiCN0AAAAASUVORK5CYII=";
        private static string IconCopy64Base64 = "iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAAHYYAAB2GAV2iE4EAAAIKSURBVHhe7ZihUgMxEIZTHAbFe/AgzFCPAFOLaT0CbFsDsh7fGRDwCCiQvAEzYGpAAd3rXqeT3jUbLnvJXvabyWyuor3d/vmTTc8gw+HwF6dJMJ1O1+/GyR7GbNlSwGQyKZ5jMRqNiqgKaAktAMZsCeYBz2/v5mb+Yj4X3/gJjfurE5ytEOsBt/NX7+RTIFgBPhZfOJOFegDGxh5wfDnH2Qp7bVPRc0DLOBUQyt2pJKcAqe5OxVkAqe5OJXsP8C4ArO2qIRVVAMbgwLkARs8TcP+2dgAgewU4zwHUE975+Klyx3i47he/UX5/KpQqC6aAi/6ROTzYxyc5BFNAHbCuITbtNUJhnzR1F8CYLboLYEzeA7juHMUogKsrFVMArq5UTRCjOGBtU4YLVQDGaIC7n40f191j3eAiegFi3zl6F6Dq36kaVGLfOaoHYKyl7Ra3yslhcOEsgNQ+n4qzF2iKqxew/aLu3/bxlU3s79P7AAsxBeBahmIKwOVFWx4QmnKtcd03+KIeYLFWADeqgETRAmDMFi0AxmwRvwuUru6L7gJIZxQwGAyKuIvZbIazDBWwmfyyQz/FaXoKoFIqhaIAO/nlO9zhY/cVsCt5IHoBOK/bXMkD0QvA1edTkgeie0BTqjyAmjzQOQ/wSR7oVAF8kwc6U4D/JA+07gHc+CQPdEYBy8R/fJM3xpg/2jBVAuytoXYAAAAASUVORK5CYII=";

        public static Page GetIcon(double scaling)
        {
            return GetIcon(scaling, ref Icon32Base64, ref Icon48Base64, ref Icon64Base64, 32);
        }

        public static Page GetIconCopy(double scaling)
        {
            return GetIcon(scaling, ref IconCopy32Base64, ref IconCopy48Base64, ref IconCopy64Base64, 32);
        }

        public static Page GetIcon(double scaling, ref string icon1, ref string icon15, ref string icon2, double resolution)
        {
            byte[] bytes;

            if (scaling <= 1)
            {

                bytes = Convert.FromBase64String(icon1);
            }
            else if (scaling <= 1.5)
            {
                bytes = Convert.FromBase64String(icon15);
            }
            else
            {
                bytes = Convert.FromBase64String(icon2);
            }

            IntPtr imagePtr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, imagePtr, bytes.Length);

            RasterImage icon;

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

            Page pag = new Page(resolution, resolution);
            pag.Graphics.DrawRasterImage(0, 0, resolution, resolution, icon);

            return pag;
        }

        public static void PerformAction(int actionIndex, MainWindow window, InstanceStateData stateData)
        {
            if (window.TransformedTree == null || window.PlottingActions.Count == 0 || (stateData.Tags.TryGetValue("a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6", out object lassoTag) && (bool)lassoTag))
            {
                return;
            }
            stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = true;

            Avalonia.Controls.PanAndZoom.ZoomBorder zom = window.FindControl<Avalonia.Controls.PanAndZoom.ZoomBorder>("PlotContainer");

            Grid lassoGrid = new Grid() { ClipToBounds = true };

            lassoGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
            lassoGrid.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));

            lassoGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
            lassoGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            lassoGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
            lassoGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

            StackPanel pnl = new StackPanel() { Orientation = Avalonia.Layout.Orientation.Horizontal, Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            pnl.Children.Add(new TextBlock() { Text = "Lasso selection currently active", VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, FontSize = 13 });
            HelpButton help = new HelpButton() { Margin = new Avalonia.Thickness(10, 0, 0, 0) };
            AvaloniaBugFixes.SetToolTip(help, HelpText);
            help.PointerPressed += async (s, e) =>
            {
                HelpWindow helpWindow = new HelpWindow(Modules.LoadedModulesMetadata[Id].BuildReadmeMarkdown(), Id);
                await helpWindow.ShowDialog2(window);
            };
            pnl.Children.Add(help);
            Grid.SetColumn(pnl, 1);
            lassoGrid.Children.Add(pnl);

            Button closeButton = new Button() { Margin = new Avalonia.Thickness(5, 5, 10, 0), Width = 32, Height = 32, Background = Avalonia.Media.Brushes.Transparent, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top, Content = new Avalonia.Controls.Shapes.Path() { Width = 10, Height = 10, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Data = Avalonia.Media.Geometry.Parse("M0,0 L10,10 M10,0 L0,10"), StrokeThickness = 2 } };
            closeButton.Classes.Add("SideBarButton");
            Grid.SetColumn(closeButton, 2);
            lassoGrid.Children.Add(closeButton);

            Canvas separator = new Canvas() { Height = 1, Margin = new Avalonia.Thickness(5, 5, 5, 1), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Bottom };
            separator.Classes.Add("RibbonSeparator");
            Grid.SetColumnSpan(separator, 4);
            Grid.SetRow(separator, 1);
            lassoGrid.Children.Add(separator);

            lassoGrid.MaxHeight = 0;

            Avalonia.Animation.Transitions openCloseTransitions = new Avalonia.Animation.Transitions();
            openCloseTransitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Avalonia.Controls.Shapes.Path.MaxHeightProperty, Duration = TimeSpan.FromMilliseconds(150) });

            lassoGrid.Transitions = openCloseTransitions;
            window.FindControl<StackPanel>("UpperBarContainer").Children.Add(lassoGrid);
            window.SetSelection(null);
            lassoGrid.MaxHeight = 80;

            Canvas can = new Canvas() { Width = window.FindControl<Canvas>("ContainerCanvas").Width, Height = window.FindControl<Canvas>("ContainerCanvas").Height };

            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
            {
                await System.Threading.Tasks.Task.Delay(150);
                lassoGrid.Transitions = null;
                lassoGrid.MaxHeight = double.PositiveInfinity;
            });

            closeButton.Click += async (s, e) =>
            {
                lassoGrid.MaxHeight = lassoGrid.Bounds.Height;
                lassoGrid.Transitions = openCloseTransitions;
                lassoGrid.MaxHeight = 0;

                await System.Threading.Tasks.Task.Delay(150);
                window.FindControl<StackPanel>("UpperBarContainer").Children.Remove(lassoGrid);

                window.FindControl<Canvas>("ContainerCanvas").Children.Remove(can);

                stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = false;
            };

            window.SetSelection(null);

            can.Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(255, 255, 255), 0);

            window.FindControl<Canvas>("ContainerCanvas").Children.Add(can);

            List<Point> selectionPoints = new List<Point>();

            bool globalIsClosed = false;
            Avalonia.Controls.Shapes.Path globalPath = null;

            void pointerPressed(object sender, Avalonia.Input.PointerPressedEventArgs e)
            {
                Avalonia.Point pt = e.GetCurrentPoint(can).Position;

                bool isClosed = selectionPoints.Count > 0 && ((pt.X == selectionPoints[^1].X && pt.Y == selectionPoints[^1].Y) || Math.Sqrt((pt.X - selectionPoints[0].X) * (pt.X - selectionPoints[0].X) + (pt.Y - selectionPoints[0].Y) * (pt.Y - selectionPoints[0].Y)) * zom.ZoomX <= 25);


                if (!isClosed)
                {
                    selectionPoints.Add(new Point(pt.X, pt.Y));
                }

                Page pg = new Page(can.Width, can.Height);

                if (selectionPoints.Count > 1)
                {
                    GraphicsPath pth = new GraphicsPath();

                    for (int i = 0; i < selectionPoints.Count; i++)
                    {
                        if (i == 0)
                        {
                            pth.MoveTo(selectionPoints[i]);
                        }
                        else
                        {
                            pth.LineTo(selectionPoints[i]);
                        }
                    }

                    if (isClosed)
                    {
                        pth.Close();
                    }

                    pg.Graphics.StrokePath(pth, window.SelectionColour, lineWidth: 5 / zom.ZoomX, lineCap: LineCaps.Round, lineJoin: LineJoins.Round, tag: "selectionOutline");
                }
                else if (selectionPoints.Count == 1)
                {
                    pg.Graphics.StrokePath(new GraphicsPath().MoveTo(selectionPoints[0]).LineTo(selectionPoints[0]), window.SelectionColour, lineWidth: 5 / zom.ZoomX, lineCap: LineCaps.Round, lineJoin: LineJoins.Round, tag: "selectionOutline");
                }



                can.Children.Clear();
                can.Children.Add(pg.PaintToCanvas(new Dictionary<string, Delegate>()
                {
                    {
                        "selectionOutline",
                        new Action<Avalonia.Controls.Shapes.Path>((Avalonia.Controls.Shapes.Path path) =>
                        {
                            void zoomHandler(object sender, Avalonia.AvaloniaPropertyChangedEventArgs e)
                            {
                                if (e.Property == Avalonia.Controls.PanAndZoom.ZoomBorder.ZoomXProperty)
                                {
                                    path.StrokeThickness = 5 / zom.ZoomX;
                                }
                            };

                            if (isClosed)
                            {
                                globalIsClosed = isClosed;
                                globalPath = path;
                            }

                            zom.PropertyChanged += zoomHandler;

                            can.DetachedFromLogicalTree += (s, e) =>
                            {
                                zom.PropertyChanged -= zoomHandler;
                            };
                        })
                    }
                }));
            };



            void pointerReleased(object sender, Avalonia.Input.PointerReleasedEventArgs e)
            {
                bool isClosed = globalIsClosed;
                Avalonia.Controls.Shapes.Path path = globalPath;

                if (isClosed)
                {
                    SkiaSharp.SKColor selectionChildColor = window.SelectionChildSKColor;
                    List<string> tipsInside = new List<string>();
                    List<TreeNode> nodesInside = new List<TreeNode>();
                    int tipCount = 0;

                    foreach (KeyValuePair<string, Point> kvp in window.Coordinates)
                    {
                        if (path.RenderedGeometry.FillContains(new Avalonia.Point(kvp.Value.X - window.PlotOrigin.X + 10, kvp.Value.Y - window.PlotOrigin.Y + 10)))
                        {
                            TreeNode node = window.TransformedTree.GetNodeFromId(kvp.Key);
                            if (node != null)
                            {
                                nodesInside.Add(node);

                                if (node.Children.Count == 0)
                                {
                                    tipCount++;
                                }
                            }
                        }
                    }

                    Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(async () =>
                    {
                        foreach (TreeNode node in nodesInside)
                        {
                            foreach ((double, SKRenderAction) pth in MainWindow.FindPaths(window.FullSelectionCanvas, node.Id))
                            {
                                window.ChangeActionColour(pth.Item2, selectionChildColor);
                            }
                        }

                        (int nodeIndex, string attributeName) = await ShowAttributeSelectionWindow(nodesInside, tipCount, window, window.TransformedTree);

                        if (attributeName != null)
                        {
                            foreach (TreeNode node in nodesInside)
                            {
                                if (nodeIndex == 2 || (nodeIndex == 1 && node.Children.Count == 0) || (nodeIndex == 0 && node.Children.Count > 0))
                                {
                                    if (node.Attributes.TryGetValue(attributeName, out object attributeValue))
                                    {
                                        if (attributeValue is string attributeString)
                                        {
                                            tipsInside.Add(attributeString);
                                        }
                                        else if (attributeValue is double attributeDouble)
                                        {
                                            tipsInside.Add(attributeDouble.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                        }
                                    }
                                }
                            }
                        }

                        if (tipsInside.Count > 0)
                        {
                            await Avalonia.Application.Current.Clipboard.SetTextAsync(tipsInside.Aggregate((a, b) => a + "\n" + b));
                        }

                        lassoGrid.MaxHeight = lassoGrid.Bounds.Height;
                        lassoGrid.Transitions = openCloseTransitions;
                        lassoGrid.MaxHeight = 0;

                        window.HasPointerDoneSomething = true;
                        e.Handled = true;

                        path.Transitions = new Avalonia.Animation.Transitions();
                        path.Transitions.Add(new Avalonia.Animation.DoubleTransition() { Property = Avalonia.Controls.Shapes.Path.OpacityProperty, Duration = TimeSpan.FromMilliseconds(500) });
                        path.Opacity = 0;

                        await Task.Delay(550);
                        window.FindControl<StackPanel>("UpperBarContainer").Children.Remove(lassoGrid);

                        window.FindControl<Canvas>("ContainerCanvas").Children.Remove(can);

                        stateData.Tags["a04dcde8-75e2-43b5-a45b-e78ec8fd1ab6"] = false;

                        can.PointerPressed -= pointerPressed;
                        can.PointerReleased -= pointerReleased;
                    });
                }
            }

            can.PointerPressed += pointerPressed;
            can.PointerReleased += pointerReleased;
        }

        private static async Task<(int, string)> ShowAttributeSelectionWindow(List<TreeNode> selectedNodes, int selectedTipsCount, Window window, TreeNode tree)
        {
            if (selectedNodes.Count == 0)
            {
                return (1, "Name");
            }
            else
            {
                ChildWindow attributeSelectionWindow = new ChildWindow() { FontFamily = window.FontFamily, FontSize = window.FontSize, Icon = window.Icon, Width = 350, Height = 190, Title = "Select attribute...", WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(231, 231, 231)), CanMaximizeMinimize = false, SizeToContent = SizeToContent.Height };

                Grid grd = new Grid() { Margin = new Avalonia.Thickness(10) };
                attributeSelectionWindow.Content = grd;
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.RowDefinitions.Add(new RowDefinition(1, GridUnitType.Star));
                grd.RowDefinitions.Add(new RowDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                grd.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                {
                    Grid header = new Grid();
                    header.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                    header.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                    header.Children.Add(new DPIAwareBox(GetIconCopy) { Width = 32, Height = 32, Margin = new Avalonia.Thickness(0, 0, 10, 0) });

                    TextBlock blk = new TextBlock() { Text = "Copy attribute", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, Margin = new Avalonia.Thickness(0, 0, 0, 10), FontSize = 16, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(0, 114, 178)) };
                    Grid.SetColumn(blk, 1);
                    header.Children.Add(blk);

                    Grid.SetColumnSpan(header, 2);
                    grd.Children.Add(header);
                }

                {
                    TextBlock blk = new TextBlock() { Text = selectedTipsCount.ToString() + " tip" + (selectedTipsCount != 1 ? "s" : "") + ", " + selectedNodes.Count + " node" + (selectedTipsCount != 1 ? "s" : "") + " selected", HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 5, 0, 10), FontSize = 13, Foreground = new Avalonia.Media.SolidColorBrush(Avalonia.Media.Color.FromRgb(102, 102, 102)) };
                    Grid.SetColumnSpan(blk, 2);
                    Grid.SetRow(blk, 1);
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { Text = "Select attribute to copy:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 3);
                    grd.Children.Add(blk);
                }

                {
                    TextBlock blk = new TextBlock() { Text = "Copy attribute at:", FontSize = 14, Margin = new Avalonia.Thickness(0, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center };
                    Grid.SetRow(blk, 2);
                    grd.Children.Add(blk);
                }

                ComboBox nodeBox = new ComboBox() { Items = new List<string>() { "Internal nodes", "Tips", "All nodes" }, SelectedIndex = 1, Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                Grid.SetRow(nodeBox, 2);
                Grid.SetColumn(nodeBox, 1);
                grd.Children.Add(nodeBox);

                Grid buttonGrid = new Grid();
                Grid.SetColumnSpan(buttonGrid, 2);

                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(0, GridUnitType.Auto));
                buttonGrid.ColumnDefinitions.Add(new ColumnDefinition(1, GridUnitType.Star));

                Button okButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "OK", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black } };
                okButton.Classes.Add("SideBarButton");
                Grid.SetColumn(okButton, 1);
                buttonGrid.Children.Add(okButton);

                Button cancelButton = new Button() { HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center, Width = 100, Content = new TextBlock() { Text = "Cancel", FontSize = 14, Foreground = Avalonia.Media.Brushes.Black }, Foreground = Avalonia.Media.Brushes.Black };
                cancelButton.Classes.Add("SideBarButton");
                Grid.SetColumn(cancelButton, 3);
                buttonGrid.Children.Add(cancelButton);

                Grid.SetRow(buttonGrid, 5);
                grd.Children.Add(buttonGrid);

                bool result = false;

                okButton.Click += (s, e) =>
                {
                    result = true;
                    attributeSelectionWindow.Close();
                };

                cancelButton.Click += (s, e) =>
                {
                    attributeSelectionWindow.Close();
                };

                HashSet<string> attributes = new HashSet<string>();

                foreach (TreeNode node in selectedNodes)
                {
                    foreach (KeyValuePair<string, object> nodeAttribute in node.Attributes)
                    {
                        attributes.Add(nodeAttribute.Key);
                    }
                }

                List<string> attributesList = attributes.ToList();

                ComboBox attributeBox = new ComboBox() { Items = attributesList, SelectedIndex = Math.Max(attributesList.IndexOf("Name"), 0), Margin = new Avalonia.Thickness(5, 0, 0, 10), VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center, MinWidth = 150, FontSize = 14, HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch };
                Grid.SetRow(attributeBox, 3);
                Grid.SetColumn(attributeBox, 1);
                grd.Children.Add(attributeBox);


                await attributeSelectionWindow.ShowDialog2(window);

                if (result)
                {
                    return (nodeBox.SelectedIndex, attributesList[attributeBox.SelectedIndex]);
                }
                else
                {
                    return (2, null);
                }
            }

        }
    }
}
