using Avalonia;
using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VectSharp.Canvas;

namespace TreeViewer
{
    public partial class MainWindow
    {


        private void SetupPlatform()
        {
            if (Modules.IsWindows)
            {
                this.ExtendClientAreaToDecorationsHint = true;
                this.ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;
                this.ExtendClientAreaTitleBarHeightHint = -1;

                Control windowIcon;
                Control windowTree;

                Image themeLeft1x = null;
                Image themeLeft15x = null;
                Image themeLeft2x = null;

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-1x.png")))
                {
                    themeLeft1x  = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-1x.png")) };
                }

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-1.5x.png")))
                {
                    themeLeft15x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-1.5x.png")) };
                }

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-2x.png")))
                {
                    themeLeft2x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-left-2x.png")) };
                }

                if (themeLeft1x != null || themeLeft15x != null || themeLeft2x != null)
                {
                    if (themeLeft2x != null)
                    {
                        if (themeLeft15x == null)
                        {
                            themeLeft15x = themeLeft2x;
                        }

                        if (themeLeft1x == null)
                        {
                            themeLeft1x = themeLeft15x;
                        }
                    }
                    else if (themeLeft15x != null)
                    {
                        themeLeft2x = themeLeft15x;

                        if (themeLeft1x == null)
                        {
                            themeLeft1x = themeLeft15x;
                        }
                    }
                    else
                    {
                        themeLeft15x = themeLeft1x;
                        themeLeft2x = themeLeft15x;
                    }

                    themeLeft1x.Height = 66;
                    themeLeft15x.Height = 66;
                    themeLeft2x.Height = 66;

                    windowIcon = new DPIAwareBox((scaling) => scaling <= 1 ? themeLeft1x : scaling <= 1.5 ? themeLeft15x : themeLeft2x);
                }
                else
                {
                    windowIcon = new Canvas();
                }



                Image themeRight1x = null;
                Image themeRight15x = null;
                Image themeRight2x = null;

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-1x.png")))
                {
                    themeRight1x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-1x.png")) };
                }

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-1.5x.png")))
                {
                    themeRight15x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-1.5x.png")) };
                }

                if (File.Exists(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-2x.png")))
                {
                    themeRight2x = new Image() { Source = new Avalonia.Media.Imaging.Bitmap(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TreeViewer", "theme-right-2x.png")) };
                }

                if (themeRight1x != null || themeRight15x != null || themeRight2x != null)
                {
                    if (themeRight2x != null)
                    {
                        if (themeRight15x == null)
                        {
                            themeRight15x = themeRight2x;
                        }

                        if (themeRight1x == null)
                        {
                            themeRight1x = themeRight15x;
                        }
                    }
                    else if (themeRight15x != null)
                    {
                        themeRight2x = themeRight15x;

                        if (themeRight1x == null)
                        {
                            themeRight1x = themeRight15x;
                        }
                    }
                    else
                    {
                        themeRight15x = themeRight1x;
                        themeRight2x = themeRight15x;
                    }

                    themeRight1x.Height = 66;
                    themeRight15x.Height = 66;
                    themeRight2x.Height = 66;

                    windowTree = new DPIAwareBox((scaling) => scaling <= 1 ? themeRight1x : scaling <= 1.5 ? themeRight15x : themeRight2x);
                }
                else
                {
                    windowTree = VectSharp.SVG.Parser.FromStream(System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("TreeViewer.Assets.WindowTree.svg")).PaintToCanvas(false);
                }

                this.FindControl<Viewbox>("WindowIconBox").Child = windowIcon;
                this.FindControl<Viewbox>("WindowTreeBox").Child = windowTree;

                this.FindControl<Grid>("PopupLayer").Margin = new Thickness(0, 35, 0, 0);

                this.FindControl<Control>("TitleBar").PointerPressed += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(0);
                    }

                    PlatformImpl?.BeginMoveDrag(e);

                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(7);
                    }
                };

                this.FindControl<Control>("TitleBar").DoubleTapped += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                };

                this.FindControl<Control>("TitleBar2").PointerPressed += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(0);
                    }

                    PlatformImpl?.BeginMoveDrag(e);

                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.Padding = new Thickness(7);
                    }
                };

                this.FindControl<Control>("TitleBar2").DoubleTapped += (i, e) =>
                {
                    if (this.WindowState == WindowState.Maximized || this.WindowState == WindowState.FullScreen)
                    {
                        this.WindowState = WindowState.Normal;
                    }
                    else
                    {
                        this.WindowState = WindowState.Maximized;
                    }
                };

                this.PropertyChanged += (s, e) =>
                {
                    if (e.Property == MainWindow.IsActiveProperty)
                    {
                        if (!this.IsActive)
                        {
                            this.FindControl<Grid>("TitleBar").Opacity = 0.5;
                            this.FindControl<Grid>("TitleBar2").Opacity = 0.5;
                        }
                        else
                        {
                            this.FindControl<Grid>("TitleBar").Opacity = 1;
                            this.FindControl<Grid>("TitleBar2").Opacity = 1;
                        }
                    }
                };
            }
            else if (Modules.IsMac)
            {
                this.FindControl<Control>("TitleBarContainer").IsVisible = false;
                this.FindControl<Control>("TitleBarContainer2").IsVisible = false;
                this.FindControl<Grid>("TitleBarBG").RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
                this.FindControl<Grid>("TitleBarBG").Margin = new Thickness(0);
                this.FindControl<Grid>("RibbonTabContainer").Margin = new Thickness(0);
                this.FindControl<Canvas>("RibbonBarBackground").Margin = new Thickness(0);
                this.FindControl<Border>("WindowBorder").BorderThickness = new Thickness(0);

                this.PlatformImpl.GetType().InvokeMember("SetTitleBarColor", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.InvokeMethod, null, this.PlatformImpl, new object[] { Avalonia.Media.Color.FromRgb(0, 114, 178) });
            }
            else
            {
                this.FindControl<Control>("TitleBarContainer").IsVisible = false;
                this.FindControl<Control>("TitleBarContainer2").IsVisible = false;
                this.FindControl<Grid>("TitleBarBG").RowDefinitions[0].Height = new GridLength(0, GridUnitType.Pixel);
                this.FindControl<Grid>("TitleBarBG").Margin = new Thickness(0);
                this.FindControl<Grid>("RibbonTabContainer").Margin = new Thickness(0);
                this.FindControl<Canvas>("RibbonBarBackground").Margin = new Thickness(0);
                this.FindControl<Border>("WindowBorder").BorderThickness = new Thickness(0);
            }

            if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.WindowsStyle)
            {
                this.Classes.Add("WindowsStyle");
            }
            else if (GlobalSettings.Settings.InterfaceStyle == GlobalSettings.InterfaceStyles.MacOSStyle)
            {
                this.Classes.Add("MacOSStyle");
            }

            BuildRibbon();
        }
    }
}
