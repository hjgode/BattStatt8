#if DEBUG
#define DEBUG_AGENT
#endif
using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;

using System.Linq;
using Windows.Phone.Devices.Power;

using System.Windows.Media.Imaging;

//see http://msdn.microsoft.com/en-us/library/windows/apps/hh202941%28v=vs.105%29.aspx

//see http://www.geekchamp.com/tips/wp7-working-with-images-content-vs-resource-build-action

namespace ScheduledTaskAgent1
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        static ScheduledAgent()
        {
            // Subscribe to the managed exception handler
            Deployment.Current.Dispatcher.BeginInvoke(delegate
            {
                Application.Current.UnhandledException += UnhandledException;
            });
        }

        /// Code to execute on Unhandled Exceptions
        private static void UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }
        }

        public static void updateTile()
        {
            System.Diagnostics.Debug.WriteLine("updateTile()");
            Battery _battery = Battery.GetDefault();
            int iPerc = _battery.RemainingChargePercent;
            string suffix="_000";
            if (iPerc <= 30)
                suffix = "_030";
            else if (iPerc > 30 && iPerc < 50)
                suffix = "_050";
            else if (iPerc >= 50 && iPerc <70)
                suffix = "_050";
            else if (iPerc >= 70 && iPerc < 100)
                suffix = "_070";
            else if (iPerc == 100)
                suffix = "_100";

            IconicTileData itd = new IconicTileData();
            itd.Count = _battery.RemainingChargePercent;

            Uri uriSmall = new Uri("/images/IconicTileSmall"+suffix+".png", UriKind.Relative);
            Uri uriMedium = new Uri("/images/IconicTileMediumLarge" + suffix + ".png", UriKind.Relative);
            //BitmapImage imgSource = new BitmapImage(uri);

            itd.IconImage = uriMedium;

            itd.SmallIconImage = uriSmall;

            itd.Title = string.Format("{0} %", _battery.RemainingChargePercent);
            itd.WideContent1 = "Battery status:";
            itd.WideContent2 = string.Format("remaining charge: {0} %", _battery.RemainingChargePercent);
            itd.WideContent3 = string.Format("remaining time:   {0} : {1}", _battery.RemainingDischargeTime.Hours, _battery.RemainingDischargeTime.Minutes);

            //the following did not work using a FlipTile, so I stay with IconTile
            /*
            FlipTileData ftd = new FlipTileData();
            ftd.BackBackgroundImage = new Uri("/images/IconicTileMediumLarge" + suffix + ".png", UriKind.Relative);
            ftd.BackContent = string.Format("Battery status {0}", _battery.RemainingChargePercent);
            ftd.BackgroundImage = new Uri("/images/IconicTileMediumLarge" + suffix + ".png", UriKind.Relative);
            ftd.BackTitle = string.Format("Battery: {0}%", _battery.RemainingChargePercent);
            ftd.Count = _battery.RemainingChargePercent;
            ftd.SmallBackgroundImage = new Uri("/images/IconicTileSmall" + suffix + ".png", UriKind.Relative);
            ftd.Title = string.Format("Battery {0}%", _battery.RemainingChargePercent);
            ftd.WideBackBackgroundImage = new Uri("/images/FlipCycleTileLarge.png", UriKind.Relative);
            ftd.WideBackContent = "Battery status: " + 
                string.Format("remaining charge: {0} %", _battery.RemainingChargePercent) + 
                " : " + string.Format("remaining time:   {0} : {1}", _battery.RemainingDischargeTime.Hours, _battery.RemainingDischargeTime.Minutes); ;
            ftd.WideBackgroundImage = new Uri("/images/FlipCycleTileLarge.png", UriKind.Relative);
            */

            ShellTile pTile = ShellTile.ActiveTiles.First();
            pTile.Update(itd);
            /*
            try
            {
                if (pTile != null)
                {
                    if (pTile.GetType() == typeof(IconicTileData))
                        pTile.Update(itd);
                    else if (pTile.GetType() == typeof(FlipTileData))
                        pTile.Update(ftd);
                    else
                        pTile.Update(itd);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception: " + ex.Message);
            }
            */
        }

        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            //TODO: Add code to perform your task in background
            string toastMessage = "";

            string battPerc = string.Format("{0} %", Battery.GetDefault().RemainingChargePercent);
            string battTime = string.Format("{0}:{1}", Battery.GetDefault().RemainingDischargeTime.Hours, Battery.GetDefault().RemainingDischargeTime.Minutes);
            // If your application uses both PeriodicTask and ResourceIntensiveTask
            // you can branch your application code here. Otherwise, you don't need to.
            if (task is PeriodicTask)
            {
                // Execute periodic task actions here.
                toastMessage = "Periodic task running.";
                
            }
            else
            {
                // Execute resource-intensive task actions here.
                toastMessage = "Resource-intensive task running.";
            }

            toastMessage += "\n" + battPerc + "\n" + battTime;
            
            updateTile();

            // Launch a toast to show that the agent is running.
            // The toast will not be shown if the foreground application is running.
            ShellToast toast = new ShellToast();
            toast.Title = "Background Agent Sample";
            toast.Content = toastMessage;
            //toast.Show();

            // If debugging is enabled, launch the agent again in one minute.
#if DEBUG_AGENT
  ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
#endif

            NotifyComplete();
        }
    }
}