using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using MAS_GUI.Util;

namespace MAS_GUI
{
    public partial class MainWindow : Window
    {
        private bool isRunning;
        private readonly object logLock = new object();
        private readonly string logFilePath;
        private DispatcherTimer completionTimer;

        private enum OperationMode
        {
            Activation,
            Uninstall
        }

        public MainWindow()
        {
            InitializeComponent();
            logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mas_gui.log");
            
            try
            {
                if (File.Exists(logFilePath))
                {
                    File.Delete(logFilePath);
                }
            }
            catch { }

            ScriptRunner.OnLog += delegate(string text)
            {
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate(object o)
                    {
                        AppendLog((string)o);
                        return null;
                    }),
                    text);
            };
            
            // Hook up sponsor dialog event
            SponsorDialog.Closed += OnSponsorClosed;
        }

        private void OnTitleBarMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                try
                {
                    DragMove();
                }
                catch
                {
                }
            }
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OnMaximizeClick(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Normal)
            {
                WindowState = WindowState.Maximized;
            }
            else
            {
                WindowState = WindowState.Normal;
            }
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnShowAdvanced(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Collapsed;
            AdvancedPanel.Visibility = Visibility.Visible;
            AdvancedButton.Visibility = Visibility.Collapsed;
            BackButton.Visibility = Visibility.Visible;
            AnimatePanel(AdvancedPanel);
        }

        private void OnShowMain(object sender, RoutedEventArgs e)
        {
            MainPanel.Visibility = Visibility.Visible;
            AdvancedPanel.Visibility = Visibility.Collapsed;
            AdvancedButton.Visibility = Visibility.Visible;
            BackButton.Visibility = Visibility.Collapsed;
            AnimatePanel(MainPanel);
        }

        private void AnimatePanel(UIElement element)
        {
            if (element == null) return;
            element.Opacity = 0;
            DoubleAnimation animation = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(220)));
            element.BeginAnimation(OpacityProperty, animation);
        }

        private void OnRunHwid(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/HWID", "HWID Activation", output), "HWID 数字激活", OperationMode.Activation, true, false);
        }

        private void OnRunWindowsUninstall(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Z-Reset", "Windows Reset", output), "Windows 激活清除", OperationMode.Uninstall, true, false);
        }

        private void OnRunOhook(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Ohook", "Ohook Activation", output), "Ohook 永久激活", OperationMode.Activation, false, true);
        }

        private void OnRunOhookUninstall(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Ohook-Uninstall", "Ohook Uninstall", output), "Ohook 激活清除", OperationMode.Uninstall, false, true);
        }

        private void OnRunTsforgeAll(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Z-Windows /Z-Office", "TSforge Full Activation", output), "TSforge全量激活中", OperationMode.Activation, true, true);
        }

        private void OnRunTsforgeEsu(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Z-ESU", "TSforge ESU Activation", output), "TSforge ESU 激活", OperationMode.Activation, true, false);
        }

        private void OnRunKms(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Z-Windows", "KMS Activation", output), "KMS 激活", OperationMode.Activation, true, false);
        }

        private void OnRunKmsUninstall(object sender, RoutedEventArgs e)
        {
            RunInternal(output => ScriptRunner.Run("/Z-Reset", "KMS Reset", output), "KMS 激活清除", OperationMode.Uninstall, true, false);
        }

        private void RunInternal(Action<System.Text.StringBuilder> action, string name, OperationMode mode, bool targetWindows, bool targetOffice)
        {
            if (isRunning)
            {
                AppendLog("Task already running.");
                return;
            }

            isRunning = true;
            AppendLog(string.Format("Starting {0}...", name));
            ShowProgress(name);

            ThreadPool.QueueUserWorkItem(delegate
            {
                try
                {
                    System.Text.StringBuilder outputCapture = new System.Text.StringBuilder();
                    action(outputCapture);
                    string windowsResult;
                    string officeResult;
                    BuildCompletionResults(outputCapture.ToString(), mode, targetWindows, targetOffice, out windowsResult, out officeResult);
                    
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                        new System.Windows.Threading.DispatcherOperationCallback(delegate(object o)
                        {
                            AppendLog(string.Format("{0} completed.", name));
                            ShowCompletion(windowsResult, officeResult);
                            isRunning = false;
                            return null;
                        }),
                        null);
                }
                catch (Exception ex)
                {
                    Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new System.Windows.Threading.DispatcherOperationCallback(delegate(object o)
                    {
                        AppendLog(string.Format("Error in {0}: {1}", name, ex.Message));
                        AppendLog(ex.StackTrace);
                        string windowsResult;
                        string officeResult;
                        BuildFallbackResults(mode, targetWindows, targetOffice, out windowsResult, out officeResult);
                        ShowCompletion(windowsResult, officeResult);
                        isRunning = false;
                        return null;
                    }),
                    null);
                }
            });
        }


        private void ShowProgress(string title)
        {
            ActionsPanel.IsEnabled = false;
            ProgressTitle.Text = title;
            ProgressContent.Visibility = Visibility.Visible;
            CompletionPanel.Visibility = Visibility.Collapsed;
            CompletionIcon.Opacity = 0;
            CompletionText.Opacity = 0;
            CompletionDetails.Opacity = 0;
            CompletionDetails.Visibility = Visibility.Collapsed;
            CompletionCloseButton.Visibility = Visibility.Collapsed;
            WindowsResultText.Text = string.Empty;
            OfficeResultText.Text = string.Empty;
            CompletionScale.ScaleX = 0.6;
            CompletionScale.ScaleY = 0.6;
            ProgressBar.IsIndeterminate = true;
            ProgressOverlay.Visibility = Visibility.Visible;
        }

        private void ShowCompletion(string windowsResult, string officeResult)
        {
            ProgressBar.IsIndeterminate = false;
            ProgressContent.Visibility = Visibility.Collapsed;
            CompletionPanel.Visibility = Visibility.Visible;

            Storyboard storyboard = new Storyboard();

            DoubleAnimation iconOpacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(220)));
            Storyboard.SetTarget(iconOpacity, CompletionIcon);
            Storyboard.SetTargetProperty(iconOpacity, new PropertyPath("Opacity"));
            storyboard.Children.Add(iconOpacity);

            DoubleAnimation scaleX = new DoubleAnimation(0.6, 1, new Duration(TimeSpan.FromMilliseconds(320)));
            Storyboard.SetTarget(scaleX, CompletionScale);
            Storyboard.SetTargetProperty(scaleX, new PropertyPath("ScaleX"));
            storyboard.Children.Add(scaleX);

            DoubleAnimation scaleY = new DoubleAnimation(0.6, 1, new Duration(TimeSpan.FromMilliseconds(320)));
            Storyboard.SetTarget(scaleY, CompletionScale);
            Storyboard.SetTargetProperty(scaleY, new PropertyPath("ScaleY"));
            storyboard.Children.Add(scaleY);

            DoubleAnimation textOpacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(220)));
            textOpacity.BeginTime = TimeSpan.FromMilliseconds(160);
            Storyboard.SetTarget(textOpacity, CompletionText);
            Storyboard.SetTargetProperty(textOpacity, new PropertyPath("Opacity"));
            storyboard.Children.Add(textOpacity);

            bool hasDetails = HasText(windowsResult) || HasText(officeResult);
            if (hasDetails)
            {
                CompletionDetails.Visibility = Visibility.Visible;
                CompletionCloseButton.Visibility = Visibility.Visible;
                WindowsResultText.Text = windowsResult ?? string.Empty;
                OfficeResultText.Text = officeResult ?? string.Empty;

                DoubleAnimation detailsOpacity = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(220)));
                detailsOpacity.BeginTime = TimeSpan.FromMilliseconds(200);
                Storyboard.SetTarget(detailsOpacity, CompletionDetails);
                Storyboard.SetTargetProperty(detailsOpacity, new PropertyPath("Opacity"));
                storyboard.Children.Add(detailsOpacity);
            }
            else
            {
                CompletionDetails.Visibility = Visibility.Collapsed;
                CompletionCloseButton.Visibility = Visibility.Collapsed;
            }

            storyboard.Begin();

            if (completionTimer != null)
            {
                completionTimer.Stop();
            }

            completionTimer = new DispatcherTimer();
            completionTimer.Interval = TimeSpan.FromMilliseconds(hasDetails ? 2400 : 900);
            completionTimer.Tick += delegate
            {
                completionTimer.Stop();
                ProgressOverlay.Visibility = Visibility.Collapsed;
                ActionsPanel.IsEnabled = true;
            };
            if (!hasDetails)
            {
                completionTimer.Start();
            }
        }

        private void OnCloseCompletion(object sender, RoutedEventArgs e)
        {
            if (completionTimer != null)
            {
                completionTimer.Stop();
            }
            ProgressOverlay.Visibility = Visibility.Collapsed;
            ActionsPanel.IsEnabled = true;

            // Show Sponsor Dialog
            SponsorDialog.Show();
            ActionsPanel.IsEnabled = false; // Disable main content
        }

        private void OnSponsorClosed(object sender, EventArgs e)
        {
            ActionsPanel.IsEnabled = true;
        }

        private void BuildCompletionResults(string output, OperationMode mode, bool targetWindows, bool targetOffice, out string windowsResult, out string officeResult)
        {
            if (mode == OperationMode.Activation)
            {
                BuildActivationStatus(output, out windowsResult, out officeResult);
                if (!targetWindows)
                {
                    windowsResult = "Windows：不适用";
                }
                if (!targetOffice)
                {
                    officeResult = "Office：不适用";
                }
                return;
            }

            BuildUninstallStatus(output, targetWindows, targetOffice, out windowsResult, out officeResult);
        }

        private void BuildFallbackResults(OperationMode mode, bool targetWindows, bool targetOffice, out string windowsResult, out string officeResult)
        {
            if (mode == OperationMode.Activation)
            {
                windowsResult = targetWindows ? "Windows：激活失败" : "Windows：不适用";
                officeResult = targetOffice ? "Office：激活失败" : "Office：不适用";
                return;
            }

            windowsResult = targetWindows ? "Windows：卸载失败" : "Windows：不适用";
            officeResult = targetOffice ? "Office：卸载失败" : "Office：不适用";
        }

        private void BuildUninstallStatus(string output, bool targetWindows, bool targetOffice, out string windowsResult, out string officeResult)
        {
            windowsResult = targetWindows ? "Windows：未检测到结果" : "Windows：不适用";
            officeResult = targetOffice ? "Office：未检测到结果" : "Office：不适用";

            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            bool windowsSuccess = false;
            bool windowsFailure = false;
            bool officeSuccess = false;
            bool officeFailure = false;
            bool officeNotInstalled = false;

            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                string lower = line.ToLowerInvariant();

                if (!officeSuccess && lower.Contains("successfully uninstalled ohook activation"))
                {
                    officeSuccess = true;
                }
                if (!officeFailure && lower.Contains("failed to uninstall ohook activation"))
                {
                    officeFailure = true;
                }
                if (!officeNotInstalled && lower.Contains("ohook activation is not installed"))
                {
                    officeNotInstalled = true;
                }

                if (!windowsSuccess && lower.Contains("reset process has been successfully done"))
                {
                    windowsSuccess = true;
                }
                if (!windowsFailure && lower.Contains("reset failed"))
                {
                    windowsFailure = true;
                }
                if (!windowsSuccess && lower.Contains("successfully removed kms38 protection"))
                {
                    windowsSuccess = true;
                }
                if (!windowsFailure && lower.Contains("failed to remove kms38 protection"))
                {
                    windowsFailure = true;
                }
                if (!windowsSuccess && lower.Contains("successfully cleared kms cache"))
                {
                    windowsSuccess = true;
                }
                if (!windowsSuccess && lower.Contains("uninstalling other/grace keys") && lower.Contains("[successful]"))
                {
                    windowsSuccess = true;
                }
                if (!windowsFailure && lower.Contains("uninstalling other/grace keys") && lower.Contains("[failed]"))
                {
                    windowsFailure = true;
                }
            }

            if (targetWindows)
            {
                if (windowsSuccess)
                {
                    windowsResult = "Windows：卸载完成";
                }
                else if (windowsFailure)
                {
                    windowsResult = "Windows：卸载失败";
                }
            }

            if (targetOffice)
            {
                if (officeSuccess)
                {
                    officeResult = "Office：卸载完成";
                }
                else if (officeFailure)
                {
                    officeResult = "Office：卸载失败";
                }
                else if (officeNotInstalled)
                {
                    officeResult = "Office：未安装 Ohook";
                }
            }
        }

        private void BuildActivationStatus(string output, out string windowsResult, out string officeResult)
        {
            windowsResult = "Windows：未检测到结果";
            officeResult = "Office：未检测到结果";

            if (string.IsNullOrEmpty(output))
            {
                return;
            }

            bool windowsSuccess = false;
            bool windowsFailure = false;
            bool officeSuccess = false;
            bool officeFailure = false;
            bool windowsKms38 = false;

            string[] lines = output.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string rawLine in lines)
            {
                string line = rawLine.Trim();
                if (line.Length == 0)
                {
                    continue;
                }

                string lower = line.ToLowerInvariant();

                if (!officeSuccess && lower.Contains("office is permanently activated"))
                {
                    officeSuccess = true;
                }
                if (!officeFailure && lower.Contains("office") && (lower.Contains("activation failed") || lower.Contains("errors were detected")))
                {
                    officeFailure = true;
                }

                if (!windowsSuccess && lower.Contains("kms38") && lower.Contains("activated"))
                {
                    windowsSuccess = true;
                    windowsKms38 = true;
                }
                if (!windowsSuccess && lower.Contains("permanently activated") && !lower.Contains("office"))
                {
                    windowsSuccess = true;
                }
                if (!windowsFailure && lower.Contains("activation failed") && !lower.Contains("office"))
                {
                    windowsFailure = true;
                }
                if (!windowsSuccess && lower.Contains("windows is permanently activated"))
                {
                    windowsSuccess = true;
                }
            }

            if (windowsSuccess)
            {
                windowsResult = windowsKms38 ? "Windows：已激活（KMS38）" : "Windows：已激活";
            }
            else if (windowsFailure)
            {
                windowsResult = "Windows：激活失败";
            }

            if (officeSuccess)
            {
                officeResult = "Office：已激活";
            }
            else if (officeFailure)
            {
                officeResult = "Office：激活失败";
            }
        }
        private string BuildActivationLine(string label, System.Collections.Generic.List<string> items)
        {
            if (items == null || items.Count == 0)
            {
                return string.Format("{0}：未激活成功", label);
            }

            return string.Format("{0}：{1}", label, string.Join("、", items.ToArray()));
        }

        private bool HasText(string value)
        {
            return !string.IsNullOrEmpty(value) && value.Trim().Length > 0;
        }

        private void AppendLog(string text)
        {
            WriteLogToFile(text);
        }

        private void WriteLogToFile(string text)
        {
            if (string.IsNullOrEmpty(logFilePath)) return;
            
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string line = string.Format("[{0}] {1}{2}", timestamp, text, Environment.NewLine);
            
            lock (logLock)
            {
                try
                {
                    File.AppendAllText(logFilePath, line);
                }
                catch
                {
                }
            }
        }
    }
}
