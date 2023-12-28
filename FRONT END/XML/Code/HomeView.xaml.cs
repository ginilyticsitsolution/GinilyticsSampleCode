using Flyhouse.UI.Dialogs.View;
using Flyhouse.UI.Dialogs.ViewModel;
using Flyhouse.UI.Model.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Controls.Primitives;
using Flyhouse.UI.Screens.ViewModel;
using Flyhouse.Globals;
using FlyHouse.Data.Data;
using System.Linq;
using FlyHouse.Data.Repository;
using System.Collections.ObjectModel;
using Flyhouse.Enum;
using System.Threading.Tasks;
using System.Data.Entity;
using Flyhouse.Utility;
using Flyhouse.Controls;
using Flyhouse.UI.Model.View;
using Flyhouse.MVVM;
using System.Collections.Generic;
using Flyhouse.Service;

namespace Flyhouse.UI.Screens.View
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    ///

    public partial class HomeView : UserControl
    {
        //System.Windows.Threading.DispatcherTimer Timer = new System.Windows.Threading.DispatcherTimer();
        //HomeViewModel CurrentHomeViewModel = new HomeViewModel();
        DialogWarningMassageViewModel WarningModel = new DialogWarningMassageViewModel();
        public HomeView()
        {
            var datacntxt = new HomeViewModel();
            DataContext = datacntxt;
            InitializeComponent();
            testaxisrresult.DragEnter += GridDraggingContentAxis_DragLeave;
            App._homes = this;
            //UnitLabel.Content = (Units)App.unitDisplay.GetValueOrDefault(1);

        }

        private void ShowHideMenu(string Storyboard, ToggleButton btnShow, StackPanel pnl)
        {
            Storyboard sb = Resources[Storyboard] as Storyboard;
            sb.Begin(pnl);

            if (btnShow.IsChecked == true)
            {
                pnl.BeginStoryboard((Storyboard)this.Resources[Storyboard]);

            }
            else
            {
                pnl.BeginStoryboard((Storyboard)this.Resources[Storyboard]);

            }
        }

        public void GridDraggingContentAxis_DragLeave(object sender, DragEventArgs e)
        {
            var dataitem = e.Data.GetData("Data");
            if (dataitem.GetType() == typeof(GlobalAxisCueViewModel))
            {
                GlobalAxisCueViewModel axis = (GlobalAxisCueViewModel)e.Data.GetData("Data");


                //offline and fault property not available
                //if (axis != null && (axis.Offline == true || axis.Faulted == true))
                //{
                //    e.Effects = DragDropEffects.None;
                //    AllowDrop = false;
                //}

                //    IEditableCollectionView items = testaxisrresult.Items; //Cast to interface
                //    if (items.CanRemove)
                //    {
                //        items.Remove(source);
                //    }
            }
            else if (dataitem.GetType() == typeof(CameraModel))
            {
                CameraModel Cameras = (CameraModel)e.Data.GetData("Data");
            }
            else
            {
                // GlobalAxisCueViewModel axis = (GlobalAxisCueViewModel)e.Data.GetData("Data");
                //offline and fault property not available

                //if (axis != null && (axis.Offline == true || axis.Faulted == true))
                //{
                //    e.Effects = DragDropEffects.None;
                //    AllowDrop = false;
                //}
            }
        }


        private void logoutbtn_Click(object sender, RoutedEventArgs e)
        {
            var myWindow = Window.GetWindow(this);
            myWindow.Closed += MainWindow_Closed;

            myWindow.Close();

        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            App.ShowID = 0;
            App.LoginView = new DialogLoginView();
            //App.LoginView.Closed += LoginWindow_Closed;
            App.LoginView.Show();
            App.LoginView.Activate();
        }
        private void LoginWindow_Closed(object sender, EventArgs e)
        {
            var loginDialog = sender as DialogLoginView;
            App.LoginLevel = loginDialog.ViewModel.LoginLevel;

            var window = new MainWindow();
            // window.Closed += (__, _) => App.Current.Shutdown();
            window.Show();
            window.Activate();
        }
        //private void btnRightMenuHide_Click(object sender, RoutedEventArgs e)
        //{
        //    ShowHideMenu("sbHideRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        //}

        //private void btnRightMenuShow_Click(object sender, RoutedEventArgs e)
        //{
        //    ShowHideMenu("sbShowRightMenu", btnRightMenuHide, btnRightMenuShow, pnlRightMenu);
        //}


        private async void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            //usernameLabel.Content = string.IsNullOrEmpty(DialogLoginViewModel.userName) ? "" : "USER NAME: " + DialogLoginViewModel.userName;
            //userLevelLabel.Content = "Level: "+ App.mainRole.Name;
            //Stk1.IsEnabled = false;
            //ServerStatus.Content = "Server Status : " + "Connected" ;
            var testing = new HomeViewModel();
            try
            {
                if (App.MainRole != null)
                {
                    if (App.MainRole.Id == 4 || App.MainRole.Id == 5)     // for Restricted or observer User
                    {
                        testaxisrresult.IsEnabled = false;
                        cuelistresult.IsEnabled = false;
                    }
                }
                else
                {
                    testaxisrresult.IsEnabled = true;
                    cuelistresult.IsEnabled = false;
                }
                var objFlyhouse = new FlyhouseEntities();
                var gsetup = objFlyhouse.GeneralSetups.Where(w => w.UserId == App.CurrentUser.UserID).FirstOrDefault();

                if ((gsetup == null || gsetup.IsSecondaryScreen == false || gsetup.IsVisualizer == false))
                {
                    dragpnl.Visibility = Visibility.Visible;
                    viewScreensbtn.Visibility = Visibility.Collapsed;
                    gridSize.RowDefinitions[1].Height = new GridLength(1, GridUnitType.Star);
                }
                else
                {
                    dragpnl.Visibility = Visibility.Collapsed;
                    viewScreensbtn.Visibility = Visibility.Visible;
                    gridSize.RowDefinitions[1].Height = new GridLength(0);

                }
                var dataContext = this.DataContext as HomeViewModel;
                if (dataContext != null)
                {
                    try
                    {
                        await dataContext.InitializeActiveShow();
                    }
                    catch (Exception ex)
                    {
                        // GlobalManager.Instance.SetLoadingMode(false);
                        GlobalsUtility.SaveErrorLog(ex, "InitializeActiveShow");
                        GlobalManager.Instance.IsShowRunning = false;
                        //  await App.ShowDialog(DialogMessageViewModel.OK("Error", "Sorry, there was some issue getting active show information. Please check for API Connectivity."));
                    }
                }
            }
            finally
            {
                GlobalManager.Instance.SetLoadingMode(false);
            }
        }

        private void btnTopMenuShow_Checked(object sender, RoutedEventArgs e)
        {
            //ShowHideMenu("sbShowTopMenu", btnTopMenuShow, pnlTopMenu);
            //pnlTopMenu.Visibility = Visibility.Visible;
            //// pnlborder.Child = null;
            //if(pnlborder.Child==null)
            // pnlborder.Child = new DraggablePanel() { Screens= new List<ViewModelBase>() { dragpnl.Screens[0] } };

        }

        private void btnTopMenuShow_Unchecked(object sender, RoutedEventArgs e)
        {
            //ShowHideMenu("sbHideTopMenu", btnTopMenuShow, pnlTopMenu);
            //pnlTopMenu.Visibility = Visibility.Collapsed;

        }

        private void btnRightMenuShow_Checked(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbShowRightMenu", btnRightMenuShow, pnlRightMenu);
            pnlRightMenu.Visibility = Visibility.Visible;
            // DialogWarningMassageViewModel WarningModel = new DialogWarningMassageViewModel();
            if (pnlrightborder.Child == null)
                pnlrightborder.Child = new DialogWarningMassageView() { DataContext = WarningModel };

        }

        private void btnRightMenuShow_Unchecked(object sender, RoutedEventArgs e)
        {
            ShowHideMenu("sbHideRightMenu", btnRightMenuShow, pnlRightMenu);
            pnlRightMenu.Visibility = Visibility.Collapsed;


        }

        private async void stkJog_Drop(object sender, DragEventArgs e)
        {
            //if (GlobalManager.Instance.IsShowRunning == true)
            //{
            //    await App.ShowDialog(DialogMessageViewModel.OK("Warning", Constants.ShowActivatedMessage, "OK"));
            //    return;
            //}

            base.OnDrop(e);
            HomeViewModel viewModel = this.DataContext as HomeViewModel;
            if (viewModel != null)
            {
                viewModel.IsJoystickViewOpen = true;
            }
            //var cuejoylist = new ObservableCollection<ShowCue>();
            //cuejoylist.Add(new ShowCue() { CueName = "JoyCue1", ShowID = 1, CueId = 1, CueNumber = 1 });
            //cuejoylist.Add(new ShowCue() { CueName = "JoyCue2", ShowID = 1, CueId = 2, CueNumber = 2 });

            var dataitem = e.Data.GetData("Data");
            if (dataitem.GetType() == typeof(GlobalAxisCueViewModel))
            {

                var cue = viewModel.JoyCues.FirstOrDefault();
                if (cue == null)
                {
                    return;
                }
                bool isAdding = false;
                GlobalAxisCueViewModel axisItem = (GlobalAxisCueViewModel)e.Data.GetData("Data");
                if (axisItem.IsJogPart == true)
                {
                    viewModel.IsJoystickViewOpen = false;
                    var dialog = await App.ShowDialog(DialogMessageViewModel.YesNo("Warning", "This Axis already in jog mode do want to reset it"), true);
                    if (dialog.Response == DialogMessageResponse.Button1)
                    {
                        var repo = new JoyStickRepository();
                        repo.ResetJogAxis((int)axisItem.AxisID);
                        var Data = await (new JoystickService()).UpdatePersistentAxis();
                        return;
                    }
                }

                List<GlobalAxisCueViewModel> axisItems = new List<GlobalAxisCueViewModel>();

                if (axisItem.IsFaultGroup == true && axisItem.FalutGroupData?.Count > 0 && axisItem.IsJogPart == false)
                {
                    var faultAxes = axisItem.FalutGroupData.Where(f => cue.Axes.Any(s => s.AxisID == f.AxisID) == false);
                    foreach (var a in faultAxes)
                    {
                        axisItems.Add(a);
                    }
                }
                else if (axisItem.IsJogPart == false)
                {
                    axisItems.Add(axisItem);
                }
                foreach (var showAxis in axisItems)
                {

                    isAdding = cue.Axes.Any(f => f.AxisID == showAxis.AxisID);
                    if (isAdding == false)
                    {
                        GlobalAxisView queAxis = new GlobalAxisView();
                        var queRepository = new QueRepository();
                        queAxis.AxisID = (int)showAxis.AxisID;
                        queAxis.CueId = cue.CueID;
                        if (App._commands?.showIDLabel?.Content != null && App._commands?.showIDLabel?.Content.ToString() != "")
                            queAxis.ShowID = Convert.ToInt32(App._commands.showIDLabel.Content.ToString());
                        else
                            queAxis.ShowID = showAxis.ShowID;

                        queAxis.DisplayName = showAxis.DisplayName;
                        queAxis.DisplayNumber = showAxis.DisplayNumber;
                        queAxis.AxisNumber = showAxis.AxisNumber;
                        queAxis.AxisName = showAxis.AxisName;
                        queAxis.ShowCueAxisConsoleCreatedBy = DialogLoginViewModel.userId;

                        queAxis.TargetPosition = showAxis.CurrentTargetPostion;
                        queAxis.TargetVelocity = (int?)showAxis.CurrentVelocity;
                        queAxis.TargetAcceleration = showAxis.CurrentAcceleration;

                        queAxis.TargetDeceleration = showAxis.CurrentDeceleration;

                        var id = new JoyStickRepository().InsertJoystickCueAxis(queAxis);
                        var vm = new CueAxisViewModels(new Flyhouse.Model.CueAxisModel(showAxis, cue.CueID), true);
                        vm.IsAxisInVenue = showAxis.IsAxisInVenue;
                        vm.IsRemoteMode = showAxis.IsRemoteMode;
                        if (vm != null && vm.IsGroupLeadAxis == true)
                        {
                            vm.LoadFollowers();
                        }
                        cue.AddAxisToCueCommand?.Execute(vm);
                    }
                    else
                    {
                        AlreadyExitErrorMessage();
                    }
                }
            }
            else
            {
                var cue = viewModel.JoyCues.FirstOrDefault();
                if (cue == null)
                {
                    return;
                }
                bool isAdding = false;
                CueAxisViewModels cueAxis = (CueAxisViewModels)e.Data.GetData("Data");
                isAdding = cue.Axes.Any(f => f.AxisID == cueAxis.AxisID);
                if (isAdding == false)
                {
                    GlobalAxisView queAxis = new GlobalAxisView();
                    var queRepository = new QueRepository();
                    queAxis.AxisID = (int)cueAxis.AxisID;
                    queAxis.CueId = cue.CueID;
                    queAxis.ShowID = App._commands?.showIDLabel?.Content != null ? (int)App._commands.showIDLabel.Content : 0;
                    queAxis.DisplayName = cueAxis.DisplayName;
                    queAxis.DisplayNumber = cueAxis.DisplayNumber;
                    queAxis.AxisNumber = cueAxis.AxisNum;
                    queAxis.AxisName = cueAxis.AxisName;
                    //queaxisConsole.MotorID =
                    queAxis.ShowCueAxisConsoleCreatedBy = DialogLoginViewModel.userId;
                    //queaxis.CueType = axis.type
                    //queaxis.CueTime = axis.CueTargetTime;
                    queAxis.TargetPosition = cueAxis.TargetPosition;
                    queAxis.TargetVelocity = (int?)cueAxis.TargetVelocity;
                    queAxis.TargetAcceleration = cueAxis.CurrentAcceleration;
                    //queaxis.AccelerationRamp =axis.ramp
                    queAxis.TargetDeceleration = cueAxis.CurrentDeceleration;
                    queAxis.JoystickID = cue.CueID;
                    var cueinfo = (DataContext as Cue);

                    var id = new JoyStickRepository().InsertJoystickCueAxis(queAxis);

                    var vm = new CueAxisViewModels(new Flyhouse.Model.CueAxisModel(cueAxis, cue.CueID), true);
                    vm.IsAxisInVenue = cueAxis.IsAxisInVenue;
                    vm.IsRemoteMode = cueAxis.IsRemoteMode;
                    if (vm != null && vm.IsGroupLeadAxis == true)
                    {
                        vm.LoadFollowers();
                    }
                    cue.AddAxisToCueCommand?.Execute(vm);
                }
                else
                {
                    AlreadyExitErrorMessage();
                }
            }


            // var cues = new QueRepository().getQueList().Take(1);
            //foreach (var item in cuejoylist)
            //{


            //    var c = new Cue(item, 1);

            //    var dataitem = e.Data.GetData("Data");
            //    if (dataitem.GetType() == typeof(GlobalAxisCueViewModel))
            //    {
            //        GlobalAxisCueViewModel showAxis = (GlobalAxisCueViewModel)e.Data.GetData("Data");
            //        GlobalAxisView queAxis = new GlobalAxisView();
            //        var queRepository = new QueRepository();
            //        queAxis.AxisID = (int)showAxis.AxisID;
            //        queAxis.CueId = c.CueID;
            //        if (App._commands?.showIDLabel?.Content != null && App._commands?.showIDLabel?.Content.ToString() != "")
            //            queAxis.ShowID = Convert.ToInt32(App._commands.showIDLabel.Content.ToString());
            //        else
            //            queAxis.ShowID = showAxis.ShowID;

            //        queAxis.DisplayName = showAxis.DisplayName;
            //        queAxis.DisplayNumber = showAxis.DisplayNumber;
            //        queAxis.AxisNumber = showAxis.AxisNumber;
            //        queAxis.AxisName = showAxis.AxisName;
            //        //queaxisConsole.MotorID =
            //        queAxis.ShowCueAxisConsoleCreatedBy = DialogLoginViewModel.userId;
            //        //queaxis.CueType = axis.type
            //        //queaxis.CueTime = axis.CueTargetTime;
            //        queAxis.TargetPosition = showAxis.CurrentTargetPostion;
            //        queAxis.TargetVelocity = (int?)showAxis.CurrentVelocity;
            //        queAxis.TargetAcceleration = showAxis.CurrentAcceleration;
            //        //queaxis.AccelerationRamp =axis.ramp
            //        queAxis.TargetDeceleration = showAxis.CurrentDeceleration;
            //        //queaxis.DecelerationRamp =
            //        //queAxis.joystickID = item.joystickID;
            //        ////queaxisobj.JoystickAxis =
            //        ////queaxisobj.SupplementalDeadman=
            //        //queaxisobj.CueAisSyncID = axis.CueAisSyncID;

            //        var id = new JoyStickRepository().InsertJoystickCueAxis(queAxis);
            //        var vm = new CueAxisViewModels(new Flyhouse.Model.CueAxisModel(showAxis, item.CueId));
            //        c.AddAxisToCueCommand?.Execute(vm);
            //    }
            //}
            //else
            //{
            //    CueAxisViewModels cueAxis = (CueAxisViewModels)e.Data.GetData("Data");
            //    GlobalAxisView queAxis = new GlobalAxisView();
            //    var queRepository = new QueRepository();
            //    queAxis.AxisID = (int)cueAxis.AxisID;
            //    queAxis.Cueld = cues.FirstOrDefault().Cueld;
            //    queAxis.ShowID = App._commands?.showIDLabel?.Content != null ? (int)App._commands.showIDLabel.Content : 0;
            //    queAxis.DisplayName = cueAxis.DisplayName;
            //    queAxis.DisplayNumber = cueAxis.DisplayNumber;
            //    queAxis.AxisNumber = cueAxis.AxisNum;
            //    queAxis.AxisName = cueAxis.AxisName;
            //    //queaxisConsole.MotorID =
            //    queAxis.ShowCueAxisConsoleCreatedBy = DialogLoginViewModel.userId;
            //    //queaxis.CueType = axis.type
            //    //queaxis.CueTime = axis.CueTargetTime;
            //    queAxis.TargetPosition = cueAxis.TargetPosition;
            //    queAxis.TargetVelocity = (int?)cueAxis.Velocity;
            //    queAxis.TargetAcceleration = cueAxis.CurrentAcceleration;
            //    //queaxis.AccelerationRamp =axis.ramp
            //    queAxis.TargetDeceleration = cueAxis.CurrentDeceleration;
            //    //queaxis.DecelerationRamp =
            //    ////queaxisobj.JoystickID =
            //    ////queaxisobj.JoystickAxis =
            //    ////queaxisobj.SupplementalDeadman=
            //    //queaxisobj.CueAisSyncID = axis.CueAisSyncID;
            //   // var listitem = (DataContext as Cue).Axes.Where(w => w.AxisID == cueAxis.AxisID && w.QueId == (DataContext as Cue).CueID).FirstOrDefault();
            //   // if (listitem == null)
            //  //  {
            //       // var id = queRepository.InsertAxiSetupCue(queAxis);
            //        var vm = new CueAxisViewModels(new Flyhouse.Model.CueAxisModel(cueAxis, (DataContext as Cue).CueID));
            //       (DataContext as Cue)?.AddAxisToCueCommand?.Execute(vm);
            //  //  }
            //}
            //var dialog = new DialogJoystickExpandedViewModel( new ObservableCollection<ShowCue> (cuejoylist.ToList() ));
            //await App.ShowDialog(dialog);
            //bottomGrid.Visibility = Visibility.Visible;
        }

        private async void AlreadyExitErrorMessage()
        {
            await App.ShowDialog(DialogMessageViewModel.OK("Warning ! ", " This Axis Already Exist in this JOG, please choose another one."));
        }

        public CommandsView _commands;

        private async void ErrorMessageForShowID()
        {
            await App.ShowDialog(DialogMessageViewModel.OK("Warning ! ", "You do not have any Show in open state, please open any show first!"));
        }

        private void DialogCueSetupViewModel_DialogClosed(DialogBase obj)
        {
            obj.DialogClosed -= DialogCueSetupViewModel_DialogClosed;
            if (obj is DialogCueSetupViewModel dialog)
            {
                if (dialog.CueResult != null)
                {
                    var viewmodel = (HomeViewModel)this.DataContext;

                    //GlobalManager.Instance.ProcessingTask = new Action(delegate ()
                    //{
                    var selectedCue = viewmodel.Cues.FirstOrDefault(f => f.CueID == dialog.CueResult.CueID);
                    bool isAdded = false;
                    if (selectedCue != null)
                    {
                        var cueIndex = viewmodel.Cues.IndexOf(selectedCue);
                        if (cueIndex >= 0)
                        {
                            viewmodel.Cues[cueIndex] = dialog.CueResult;
                            isAdded = true;
                        }
                    }

                    if (isAdded == false)
                    {
                        viewmodel.AddCue(dialog.CueResult);
                    }

                    //GlobalManager.Instance.RunningStatus = Enum.TaskRunningStatus.Running;
                    //});
                    //GlobalManager.Instance.RunningStatus = TaskRunningStatus.WaitForProcessing;
                    // Cues.Add(dialog.CueResult);

                }
            }
        }

        private void DialogCueSetupViewModel1_DialogClosed(DialogBase obj)
        {
            obj.DialogClosed -= DialogCueSetupViewModel1_DialogClosed;
            if (obj is DialogCueSetupViewModel dialog)
            {
                if (dialog.CueResult != null)
                {
                    var viewmodel = (HomeViewModel)this.DataContext;
                    viewmodel.AddCue(dialog.CueResult);
                }
            }
        }

        private async void stkCue_Drop(object sender, DragEventArgs e)
        {
            if (GlobalManager.Instance.IsShowRunning == true)
            {
                await App.ShowDialog(DialogMessageViewModel.OK("Warning", Constants.ShowActivatedMessage, "OK"), true);
                return;
            }

            var viewmodel = (HomeViewModel)this.DataContext;
            if (App._commands == null || viewmodel == null || App.ShowID == 0)
            {
                ErrorMessageForShowID();
                return;
            }

            //else
            //{
            var dataitem = e.Data.GetData("Data");
            if (dataitem.GetType() == typeof(GlobalAxisCueViewModel))
            {
                GlobalAxisCueViewModel axisItem = (GlobalAxisCueViewModel)e.Data.GetData("Data");
                ObservableCollection<GlobalAxisCueViewModel> axisItems = new ObservableCollection<GlobalAxisCueViewModel>();

                if (axisItem.IsFaultGroup == true && axisItem.FalutGroupData?.Count > 0)
                {
                    var faultAxes = axisItem.FalutGroupData;
                    foreach (var a in faultAxes)
                    {
                        axisItems.Add(a);
                    }
                }
                else
                {
                    axisItems.Add(axisItem);
                }
                //  (DataContext as Cue)?.AddAxisToCueCommand?.Execute(vm);
                //  }
                //var dialog = new DialogNewCueViewModel(viewmodel.Cues.Count, lst, viewmodel.showID);
                var dialog = await DialogCueSetupViewModel.Create(axisItems, App.ShowID);
                dialog.DialogClosed += DialogCueSetupViewModel_DialogClosed;
                await App.ShowDialog(dialog);

            }
            else
            {
                CueAxisViewModels cueAxis = (CueAxisViewModels)e.Data.GetData("Data");

                //var vm = new CueAxisViewModels(new Flyhouse.Model.CueAxisModel(cueAxis, (DataContext as Cue).CueID));
                var lst = new ObservableCollection<CueAxisViewModels>();
                lst.Add(cueAxis);
                //  (DataContext as Cue)?.AddAxisToCueCommand?.Execute(vm);
                //  }
                //var dialog = new DialogNewCueViewModel(viewmodel.Cues.Count, lst, viewmodel.showID);
                var dialog = await DialogCueSetupViewModel.Create(lst, App.ShowID);
                //var dialog = new DialogNewCueViewModel(Cues.Count, _commands);
                dialog.DialogClosed += DialogCueSetupViewModel1_DialogClosed;
                await App.ShowDialog(dialog);

            }
        }



        private async void cuelistresult_Drop(object sender, DragEventArgs e)
        {
            if (GlobalManager.Instance.IsShowRunning == true)
            {
                var dialog = await App.ShowDialog(DialogMessageViewModel.OK("Warning", Constants.ShowActivatedMessage, "OK"), true);
                if (dialog.Response == DialogMessageResponse.Button1)
                {
                    return;
                }

            }

            var viewmodel = (HomeViewModel)this.DataContext;
            base.OnDrop(e);

            var element = e.OriginalSource as UIElement;
            if (element != null && element.FindVisualParent<ScrollingItemsControl_ForCues>() != null)
            {
                return;
            }

            var dataitem = e.Data.GetData("Data");
            if (dataitem.GetType() == typeof(CueAxisViewModels))
            {
                var axisItem = dataitem as CueAxisViewModels;
                if (axisItem != null)
                {
                    var cue = viewmodel.Cues.Where(f => f.CueID == axisItem.QueId).FirstOrDefault();
                    if (cue != null)
                    {
                        await cue.DeleteCueAxis(axisItem);
                    }

                }
            }
        }

        private async void joglistresult_Drop(object sender, DragEventArgs e)
        {
            //if (GlobalManager.Instance.IsShowRunning == true)
            //{
            //    await App.ShowDialog(DialogMessageViewModel.OK("Warning", Constants.ShowActivatedMessage, "OK"));
            //    return;
            //}

            var viewmodel = (HomeViewModel)this.DataContext;
            base.OnDrop(e);

            var element = e.OriginalSource as UIElement;

            var dataitem = e.Data.GetData("Data");
            if (dataitem.GetType() == typeof(CueAxisViewModels))
            {
                var axisItem = dataitem as CueAxisViewModels;
                if (axisItem != null)
                {
                    var cue = viewmodel.JoyCues.Where(f => f.CueID == axisItem.QueId).FirstOrDefault();
                    if (cue != null)
                    {

                        if (element != null && element.FindVisualParent<ScrollingItemsControl_For_JoystickView>() != null)
                        {
                            if (cue.AxesCameras.Count == 0)
                            {
                                cue.JogPort = null;
                                cue.SelectedAxisLinked = null;
                            }
                        }
                        else
                        {
                            await cue.DeleteCueAxis(axisItem);
                        }
                    }

                }
            }
        }
    }

    public class Variance
    {
        public string Prop { get; set; }
        public object valA { get; set; }
        public object valB { get; set; }
    }
}

