Imports System.Collections.ObjectModel
Imports System.Windows.Forms
Imports Model.Modal
Imports System.Net.Http
Imports System.Text
Imports System.Text.RegularExpressions
Imports Autentication
Imports Newtonsoft.Json
Imports TextBox = System.Windows.Controls.TextBox
Imports System.Net.Mail

Public Class MainCasaWindow

    Public topTrackrData As ObservableCollection(Of TopTrackData)
    Public openingData As ObservableCollection(Of Opening)
    Public wallintsData As ObservableCollection(Of WallIntersection)
    Public addnPostData As ObservableCollection(Of AdditionalPost)
    Public horizStrapData As ObservableCollection(Of HorizantalStrap)
    Public horizBlockData As ObservableCollection(Of HorizantalBlocking)
    Public costCalcData As ObservableCollection(Of CostCalculationsData)
    Public elementDetailData As ObservableCollection(Of ElementDetail)
    Public inputData As InputData
    Public aspectRatio As Double
    Public file As FileHandling
    Public projectPath As String


    Private shopDrawing As ShopDrawingForm
    Public g As Graphics

    Public local_Height As Integer
    Public local_width As Integer
    Dim a As New BrushConverter()
    Dim colorString As String = "#FFF0B100"
    Dim colorString1 As String = "#FF1A1945"
    Dim blueColor = "#1A1945"
    Dim orangeColor = "#F0B100"
    Dim duration As Duration = New Duration(TimeSpan.FromSeconds(0.0))

    Public BaseWidth As Double
    Public BaseHeight As Double
    Public BaseOffsetY As Double
    Public IsFreeMode As Boolean = False
    Dim baseUrl = "https://smartbuildanalytics.ginilytics.org"
    'Dim baseUrl = "http://192.168.10.108/smartbuild"

    Dim skipCalculation = False
    Dim testMode As Boolean = False
    Private Async Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

        If testMode Then
            MainGrid.Visibility = Visibility.Visible
            Return
        End If
        Dim path = ".\dbStore.json"
        Dim dataStrAccess = New DataStoreAccess()
        dataStrAccess.InitStore()
        If dataStrAccess.dataStore.FirstLoad() Then
            Return
        End If
        Dim IsLogout = Await dataStrAccess.dataStore.GetIsLogout()

        If IsLogout = "LOGOUT" Then
            userEmail.Text = Await dataStrAccess.dataStore.GetEmail()
            userPassword.Password = Await dataStrAccess.dataStore.GetPassword()
            pdKeyTxt.Text = Await dataStrAccess.dataStore.GetPdKey()
            Return
        End If
        If dataStrAccess.dataStore.FirstLoad() Then
            LoginGrid.Visibility = Visibility.Visible
            MainGrid.Visibility = Visibility.Collapsed
            Return
        End If
        If System.IO.File.Exists(path) Then
            Me.LoginGrid.Visibility = Visibility.Collapsed
            Me.MainGrid.Visibility = Visibility.Visible

            Dim data = New Model.MacAddressUpdate() With {
           .user_id = Await dataStrAccess.dataStore.GetUserId(),
          .order_id = Await dataStrAccess.dataStore.GetOrderId(),
          .product_key = Await dataStrAccess.dataStore.GetPdKey(),
          .mac_address = Await dataStrAccess.dataStore.GetMacAddress()
            }
            Me.MainGrid.Visibility = Visibility.Hidden
            Dim pass = Await dataStrAccess.dataStore.GetPassword()
            Dim email = Await dataStrAccess.dataStore.GetEmail()
            Dim jsonContent As StringContent = New StringContent(
            JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json")

            Dim result = Await HTTPAPIServices.HTTPServices.PostAPI(Of Model.LoginResponse)($"{baseUrl}/api/users/login/?user_login={email}&&user_password={pass}&&product_key={data.product_key}", jsonContent)

            If result.subscription_status = "expired" Then
                entryMsgGrid.Visibility = Visibility.Visible
                MainGrid.Visibility = Visibility.Collapsed
                entryMsgLbl.Content = "Your Trial Or Subscription has been ended!"
                Me.Show()
                Return
            End If

            If result.plan_name = "Free Trial" Then
                ''FreeTrialItems()
                ''  Me.IsFreeMode = True
                Me.MainGrid.Visibility = Visibility.Visible
            End If

            If result.status = "success" Then
                If Convert.ToInt32(result.remainingdays) <= 15 Then
                    Me.MainGrid.Visibility = Visibility.Visible
                    MsgGrid.Visibility = Visibility.Visible
                    msgLbl.Visibility = Visibility.Visible
                    MessageBox.Show("Your access period is about to expire in " + result.remainingdays + ".")
                End If

            End If
            MainGrid.Visibility = Visibility.Visible
            Return
        End If

    End Sub



    Public Sub FreeTrialItems()
        If testMode Then
            Return
        End If
        Me.saveWallBtn.IsEnabled = False
        Me.exportPdfBtn.IsEnabled = False
        Me.shopDrawingBtn.IsEnabled = False
        Me.costCalculateBtn.IsEnabled = False

        Me.costOfSteel.IsEnabled = False
        Me.costOfScrew.IsEnabled = False
        Me.screwInPerMin.IsEnabled = False
        Me.cutPerMin.IsEnabled = False
        Me.timeMemInst.IsEnabled = False
        Me.labourCostPerHr.IsEnabled = False

        Me.costOfSteel.Text = String.Empty
        Me.costOfScrew.Text = String.Empty
        Me.screwInPerMin.Text = String.Empty
        Me.cutPerMin.Text = String.Empty
        Me.timeMemInst.Text = String.Empty
        Me.labourCostPerHr.Text = String.Empty

        Me.panelWeightLb.Text = String.Empty
        Me.screws.Text = String.Empty
        Me.labourTimeHr.Text = String.Empty
        Me.materialCost.Text = String.Empty
        Me.LabourCost.Text = String.Empty
        Me.totalCost.Text = String.Empty

    End Sub

    ''' <summary>
    ''' Constructor Calling
    ''' </summary>
    Public Sub New()
        InitializeComponent()

        Me.g = New Graphics()
        Me.g.setCanvas(CanvasArea)

        shopDrawing = New ShopDrawingForm()
        shopDrawing.setUIWindow(Me)
        inputData = New InputData()
        file = New FileHandling()
        file.Init(inputData, Me)
        Me.DataContext = Me
        topTrackrData = New ObservableCollection(Of TopTrackData)()
        openingData = New ObservableCollection(Of Opening)()
        wallintsData = New ObservableCollection(Of WallIntersection)()
        addnPostData = New ObservableCollection(Of AdditionalPost)()
        horizStrapData = New ObservableCollection(Of HorizantalStrap)()
        horizBlockData = New ObservableCollection(Of HorizantalBlocking)()
        costCalcData = New ObservableCollection(Of CostCalculationsData)()
        elementDetailData = New ObservableCollection(Of ElementDetail)()
        dataGridElementDetail.ItemsSource = elementDetailData
        DataGridViewTopTrack.ItemsSource = topTrackrData
        OpeningGrid.ItemsSource = openingData
        DataGridViewIntersection.ItemsSource = wallintsData
        DataGridViewAdditionalPost.ItemsSource = addnPostData
        DataGridViewHorizantalStrap.ItemsSource = horizStrapData
        DataGridViewHorizontalBlocking.ItemsSource = horizBlockData
        DataGridViewCostCalculations.ItemsSource = costCalcData

        Me.todaydate.Content = DateTime.Now.ToShortDateString()
        ElementDetailStack.Height = 0

    End Sub

    Private Async Sub loginBtn_Click(sender As Object, e As RoutedEventArgs)

        If testMode Then
            MainGrid.Visibility = Visibility.Visible
            Return
        End If
        If userPassword.Password = "" Or userEmail.Text = "" Or pdKeyTxt.Text = "" Then
            If userPassword.Password = "" Then
                PasswordLoginMsg.Content = "All fields are required."
            End If
            If userEmail.Text = "" Then
                emailLoginMsg.Content = "All fields are required."
            End If
            If pdKeyTxt.Text = "" Then
                pdKeyMsg.Content = "All fields are required."
            End If
            Return
        End If
        If emailLoginMsg.Content <> "" Or PasswordLoginMsg.Content <> "" Or pdKeyMsg.Content <> "" Then
            Return
        End If

        Dim util = New Authentication.AuthService().util
        Dim macaddress = util.LoadMacAddress()


        Dim jsonContent As StringContent = New StringContent(
          JsonConvert.SerializeObject(New Model.Login() With {
           .user_login = userEmail.Text,
          .user_password = userPassword.Password
        }), Encoding.UTF8, "application/json")


        Dim result = Await HTTPAPIServices.HTTPServices.PostAPI(Of Model.LoginResponse)($"{baseUrl}/api/users/login/?user_login={userEmail.Text}&&user_password={userPassword.Password}&&product_key={pdKeyTxt.Text}", jsonContent)


        If result.status = "failed" Then
            entryMsgGrid.Visibility = Visibility.Visible
            entryMsgLbl.Visibility = Visibility.Visible
            entryMsgLbl.Content = result.message
            Return
        End If
        Dim user_id = result.user_id
        Dim order_id = result.order_id
        Dim plan_name = result.plan_name
        Dim plan_id = result.plan_id
        Dim start_date = result.startdate
        Dim end_date = result.enddate
        Dim remaining_days = result.remainingdays

        Dim jsonContentForMacaddress As StringContent = New StringContent(
          JsonConvert.SerializeObject(New Model.MacAddressUpdate() With {
           .user_id = user_id,
          .order_id = order_id,
          .product_key = pdKeyTxt.Text,
          .mac_address = macaddress
        }), Encoding.UTF8, "application/json")

        Dim updateMacAddress = Await HTTPAPIServices.HTTPServices.PostAPI(Of Model.MacAddressUpdateResponse)($"{baseUrl}/api/users/updatemacid/?user_id={user_id}&&order_id={order_id}&&activation_key={pdKeyTxt.Text}&&machine_id={macaddress}", jsonContentForMacaddress)

        Dim dataStrAccess = New DataStoreAccess()

        If result.status = "success" And updateMacAddress.status.ToLower() = "ok" And result.subscription_status = "active" Then


            Dim mail = userEmail.Text
            Dim user_password = userPassword.Password
            Dim uniqKey = pdKeyTxt.Text

            Await dataStrAccess.dataStore.UpdateData(Meta.MacAddress, New Authentication.AuthService().util.LoadMacAddress())
            Await dataStrAccess.dataStore.UpdateData(Meta.Email, mail)
            Await dataStrAccess.dataStore.UpdateData(Meta.Password, user_password)
            Await dataStrAccess.dataStore.UpdateData(Meta.PdKey, uniqKey)
            Await dataStrAccess.dataStore.UpdateData(Meta.UserId, user_id)
            Await dataStrAccess.dataStore.UpdateData(Meta.OrderId, order_id)
            Await dataStrAccess.dataStore.UpdateData(Meta.PlanId, plan_id)
            Await dataStrAccess.dataStore.UpdateData(Meta.PlanName, plan_name)
            Await dataStrAccess.dataStore.UpdateData(Meta.StartDate, start_date)
            Await dataStrAccess.dataStore.UpdateData(Meta.EndDate, end_date)
            Await dataStrAccess.dataStore.UpdateData(Meta.RemainingDays, remaining_days)
            Await dataStrAccess.dataStore.UpdateData(Meta.IsLogout, "False")
            dataStrAccess.SyncData()

            Me.LoginGrid.Visibility = Visibility.Collapsed
            If plan_name = "Free Trial" Then
                FreeTrialItems()
                IsFreeMode = True
            End If
            ''(1,15)
            If Convert.ToInt32(result.remainingdays) <= 15 And Convert.ToInt32(result.remainingdays) > 0 Then
                MsgGrid.Visibility = Visibility.Visible
                MainGrid.Visibility = Visibility.Visible
                entryMsgLbl.Visibility = Visibility.Visible
                msgLbl.Visibility = Visibility.Visible
                MessageBox.Show("Your access period is about to expire in " + result.remainingdays + ".")
                Me.Show()
                Return
            End If

        Else
            If updateMacAddress.status = "installed" Then
                entryMsgGrid.Visibility = Visibility.Visible
                entryMsgLbl.Visibility = Visibility.Visible
                entryMsgLbl.Content = updateMacAddress.message
                Await dataStrAccess.dataStore.UpdateData(Meta.IsLogout, "LOGOUT")
                Me.Show()
                Return
            End If
        End If

        If result.subscription_status = "expired" Then
            entryMsgGrid.Visibility = Visibility.Visible
            Me.LoginGrid.Visibility = Visibility.Collapsed
            MainGrid.Visibility = Visibility.Collapsed
            entryMsgLbl.Content = "Your Trial or Subscription has been ended!"
            Me.Show()
            Return

        End If
        MainGrid.Visibility = Visibility.Visible
    End Sub

#Region "Validation"
    Private Sub email_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim mail = sender.Text
        Dim message As String

        Dim regex As Regex = New Regex("^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$")
        Dim isValid As Boolean = regex.IsMatch(sender.Text.Trim)

        If Not isValid Then
            message = "Email must be of type 'abc123@xy.def'."
        Else
            message = ""
        End If

        If sender.Name = "userEmail" Then
            emailLoginMsg.Content = message
        ElseIf sender.Name = "email" Then

        ElseIf sender.Name = "emailForgetTxt" Then

        End If
    End Sub

    Public Function ValidateEmail(ByVal strCheck As String) As Boolean
        Try
            Dim vEmailAddress As New System.Net.Mail.MailAddress(strCheck)
        Catch ex As Exception
            Return False
        End Try
        Return True
    End Function

    Private Sub passwordTxt_LostFocus(sender As Object, e As RoutedEventArgs)
        Dim password As String = sender.Password
        Dim message = IsPasswordValid(password)

        If sender.Name = "userPassword" Then
            PasswordLoginMsg.Content = message
        ElseIf sender.Name = "passwordTxt" Then

        End If
    End Sub

    Private Function IsPasswordValid(password As String) As String
        Dim specialCharacters As String = "!@#$%^&*()-_=+[]{}|;:'\,.<>?/"
        Dim result As String
        If password.Length < 8 Then
            result = "Password must be 8 character long."
        ElseIf Not password.Any(Function(c) Char.IsDigit(c)) Then
            result = "Password must have one digit."
        ElseIf password.Contains(" ") Then
            result = "White spaces are Not allowed."
        ElseIf Not password.Any(Function(c) Char.IsUpper(c)) Then
            result = "Password must have one Uppercase letter."
        ElseIf Not password.Any(Function(c) specialCharacters.Contains(c)) Then
            result = "Password must contain one special character."
        ElseIf password Is Nothing Then
            result = "Password is required."
        Else
            result = ""
        End If
        Return result
    End Function

    Private Sub pdKeyTxt_TextChanged(sender As Object, e As TextChangedEventArgs)
        Dim pdkey As String = sender.Text
        Dim message As String
        If pdkey.Length = 32 Then
            message = ""
        Else
            message = "Product Key must be 32 character long."
        End If

        pdKeyMsg.Content = message
    End Sub

    Private Sub NumericTextBox_PreviewTextInput(sender As Object, e As TextCompositionEventArgs)
        Dim textBox As TextBox = CType(sender, TextBox)
        Dim text As String = textBox.Text.Insert(textBox.SelectionStart, e.Text)

        If Not IsNumeric(text) AndAlso text <> "-" AndAlso text <> "." Then
            e.Handled = True
            MessageBox.Show("Invalid numeric input.")
        End If
    End Sub

    Private Sub TextBox_LostFocus(sender As Object, e As RoutedEventArgs)
        Dim textBox As TextBox = DirectCast(sender, TextBox)
        Dim inputValue As String = textBox.Text

        If Not String.IsNullOrEmpty(inputValue) AndAlso IsNumeric(inputValue) Then
            Dim numericValue As Double = Convert.ToDouble(inputValue)
            If numericValue >= 6 Then

            Else
                MessageBox.Show("Value must be greater than 6.")
                textBox.Clear()
                textBox.Focus()
                Return
            End If
        Else
            MessageBox.Show("Invalid input. Please enter a numeric value.")
            textBox.Clear()
            Return
        End If

    End Sub
    Private Sub txtBox_GotFocus(sender As Object, e As RoutedEventArgs)
        closeFileDropDown()
        Try
            If TypeOf sender Is TextBox Then
                Dim textBox As TextBox = DirectCast(sender, TextBox)
                textBox.SelectAll()
            End If
        Catch ex As Exception
            Return
        End Try

    End Sub
#End Region

    Protected Overrides Sub OnRenderSizeChanged(sizeInfo As SizeChangedInfo)
        closeFileDropDown()

        MyBase.OnRenderSizeChanged(sizeInfo)
        aspectRatio = sizeInfo.NewSize.Width / sizeInfo.NewSize.Height
        local_Height = 8.268 * 96 '(65 * sizeInfo.NewSize.Height) / 100
        local_width = 11.693 * 96 '(70 * sizeInfo.NewSize.Width) / 100
        ' Me.CanvasArea.Height = (90 * sizeInfo.NewSize.Height) / 100

        If local_width <= 943 Then
            Toggle_InBtn.RaiseEvent(New RoutedEventArgs(Toggle_InBtn.ClickEvent))
        End If

        If local_width > 943 Then
            Toggle_OutBtn.RaiseEvent(New RoutedEventArgs(Toggle_OutBtn.ClickEvent))
        End If

        If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        End If

        If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            topTrackBtn.RaiseEvent(New RoutedEventArgs(topTrackBtn.ClickEvent))
        End If

        If TryCast(costCalculateBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            costCalculateBtn.RaiseEvent(New RoutedEventArgs(costCalculateBtn.ClickEvent))
        End If
    End Sub

#Region "New"
    Private Sub newrow()
        If DataGridViewTopTrack.Visibility Then
            Dim newSerialNumber As Integer = topTrackrData.Count + 1
            topTrackrData.Add(New TopTrackData() With {.ttSNo = newSerialNumber})
        End If

        If OpeningGrid.CanUserAddRows Then
            Dim newSerialNumber As Integer = openingData.Count + 1
            openingData.Add(New Opening() With {
                            .openSNo = newSerialNumber,
                            .jambLeft = "1",
                            .jambRight = "1"
                            })
        End If

        If DataGridViewIntersection.CanUserAddRows Then
            Dim newSerialNumber As Integer = wallintsData.Count + 1
            wallintsData.Add(New WallIntersection() With {.wiSNo = newSerialNumber})
        End If

        If DataGridViewAdditionalPost.CanUserAddRows Then
            Dim newSerialNumber As Integer = addnPostData.Count + 1
            addnPostData.Add(New AdditionalPost() With {.apSNo = newSerialNumber})
        End If

        If DataGridViewHorizantalStrap.CanUserAddRows Then
            Dim newSerialNumber As Integer = horizStrapData.Count + 1
            horizStrapData.Add(New HorizantalStrap() With {.hsSNo = newSerialNumber})
        End If

        If DataGridViewHorizontalBlocking.CanUserAddRows Then
            Dim newSerialNumber As Integer = horizBlockData.Count + 1
            horizBlockData.Add(New HorizantalBlocking() With {.hbSNo = newSerialNumber})
        End If

    End Sub
#End Region

    Private Sub Window_PreviewKeyDown(sender As Object, e As Input.KeyEventArgs)
        If e.Key = Key.Escape Then
            closeBtn_Click(sender, e)
        End If
        If e.Key = Key.Space Or e.Key = Key.Enter Then
            closeFileDropDown()
        End If
        If sender.datacontext IsNot fileDropDown Then
            closeFileDropDown()
        End If

        If e.Key = Key.Delete Then
        End If

        closeElementDetail()
    End Sub

    Private Sub closeElementDetail()
        If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString And elementDetailStatck.Visibility = Visibility.Visible Then
            elementDetailBtn.RaiseEvent(New RoutedEventArgs(elementDetailBtn.UncheckedEvent))
            elementDetailBtn.IsChecked = False
        End If
    End Sub

    Private Sub closeFileDropDown()
        fileDropDown.Visibility = Visibility.Collapsed
        FileBtn.Background = a.ConvertFrom(blueColor)
        FileBtn.Foreground = a.ConvertFrom(orangeColor)
    End Sub

#Region "Open Close Grids"
    Private Sub OpenCloseFileDropDown(sender As Object, e As RoutedEventArgs)
        If fileDropDown.Visibility = Visibility.Collapsed Then
            FileBtn.Background = a.ConvertFrom(orangeColor)
            FileBtn.Foreground = a.ConvertFrom(blueColor)
            fileDropDown.Visibility = Visibility.Visible
            TopTrackStack.Visibility = Visibility.Collapsed
            OpeningGridStack.Visibility = Visibility.Collapsed
            AdditionalPostStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

        Else
            closeFileDropDown()
        End If
    End Sub

    Private Sub OpenCloseTopTrack(sender As Object, e As RoutedEventArgs)

        If TopTrackStack.Visibility = Visibility.Visible Then
            TopTrackStack.Visibility = Visibility.Collapsed
            top_track.Background = New SolidColorBrush(Colors.Transparent)

        Else
            Dim a = New BrushConverter()
            top_track.Background = a.ConvertFrom(orangeColor)
            '   Me.DataGridViewTopTrack.SelectedItems.Clear()

            TopTrackStack.Visibility = Visibility.Visible
            OpeningGridStack.Visibility = Visibility.Collapsed
            AdditionalPostStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub

    Private Sub OpenCloseOpenings(sender As Object, e As RoutedEventArgs)
        If OpeningGridStack.Visibility = Visibility.Visible Then
            OpeningGridStack.Visibility = Visibility.Collapsed
            Opening.Background = New SolidColorBrush(Colors.Transparent)

        Else
            Dim a = New BrushConverter()
            Opening.Background = a.ConvertFrom(orangeColor)
            '   Me.OpeningGrid.SelectedItems.Clear()

            OpeningGridStack.Visibility = Visibility.Visible
            TopTrackStack.Visibility = Visibility.Collapsed
            AdditionalPostStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub

    Private Sub OpenCloseAdditionalPost(sender As Object, e As RoutedEventArgs)
        If AdditionalPostStack.Visibility = Visibility.Visible Then
            AdditionalPostStack.Visibility = Visibility.Collapsed
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
        Else
            Dim a = New BrushConverter()
            additionalPost.Background = a.ConvertFrom(orangeColor)
            '    Me.DataGridViewAdditionalPost.SelectedItems.Clear()

            AdditionalPostStack.Visibility = Visibility.Visible
            OpeningGridStack.Visibility = Visibility.Collapsed
            TopTrackStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub

    Private Sub OpenCloseWallIntersection(sender As Object, e As RoutedEventArgs)
        If IntersectionStack.Visibility = Visibility.Visible Then
            IntersectionStack.Visibility = Visibility.Collapsed
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)

        Else
            Dim a = New BrushConverter()
            wall_inter.Background = a.ConvertFrom(orangeColor)
            '      Me.DataGridViewIntersection.SelectedItems.Clear()

            IntersectionStack.Visibility = Visibility.Visible
            AdditionalPostStack.Visibility = Visibility.Collapsed
            OpeningGridStack.Visibility = Visibility.Collapsed
            TopTrackStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub

    Private Sub OpenCloseHorizontalBlockings(sender As Object, e As RoutedEventArgs)
        If HorizontalBlockingStack.Visibility = Visibility.Visible Then
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            horiz_block.Background = New SolidColorBrush(Colors.Transparent)

        Else
            Dim a = New BrushConverter()
            horiz_block.Background = a.ConvertFrom(orangeColor)
            '     Me.DataGridViewHorizontalBlocking.SelectedItems.Clear()

            HorizontalBlockingStack.Visibility = Visibility.Visible
            OpeningGridStack.Visibility = Visibility.Collapsed
            TopTrackStack.Visibility = Visibility.Collapsed
            AdditionalPostStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub

    Private Sub OpenCloseHorizontalstrap(sender As Object, e As RoutedEventArgs)
        If HorizantalStrapStack.Visibility = Visibility.Visible Then

            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            HorizantalStrapStack.Visibility = Visibility.Collapsed

        Else
            Dim a = New BrushConverter()
            horiz_strap.Background = a.ConvertFrom(orangeColor)
            '     Me.DataGridViewHorizantalStrap.SelectedItems.Clear()

            HorizantalStrapStack.Visibility = Visibility.Visible
            AdditionalPostStack.Visibility = Visibility.Collapsed
            OpeningGridStack.Visibility = Visibility.Collapsed
            TopTrackStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            EndPostStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            end_post.Background = New SolidColorBrush(Colors.Transparent)

            closeFileDropDown()

        End If
    End Sub
    Private Sub OpenCloseEndPost(sender As Object, e As RoutedEventArgs)
        If EndPostStack.Visibility = Visibility.Visible Then
            EndPostStack.Visibility = Visibility.Collapsed
            end_post.Background = New SolidColorBrush(Colors.Transparent)

        Else
            Dim a = New BrushConverter()
            end_post.Background = a.ConvertFrom(orangeColor)

            EndPostStack.Visibility = Visibility.Visible
            HorizontalBlockingStack.Visibility = Visibility.Collapsed
            OpeningGridStack.Visibility = Visibility.Collapsed
            TopTrackStack.Visibility = Visibility.Collapsed
            AdditionalPostStack.Visibility = Visibility.Collapsed
            IntersectionStack.Visibility = Visibility.Collapsed
            HorizantalStrapStack.Visibility = Visibility.Collapsed

            horiz_block.Background = New SolidColorBrush(Colors.Transparent)
            wall_inter.Background = New SolidColorBrush(Colors.Transparent)
            top_track.Background = New SolidColorBrush(Colors.Transparent)
            horiz_strap.Background = New SolidColorBrush(Colors.Transparent)
            additionalPost.Background = New SolidColorBrush(Colors.Transparent)
            Opening.Background = New SolidColorBrush(Colors.Transparent)
            closeFileDropDown()

        End If
    End Sub

#End Region
    Private Sub InClose(sender As Object, e As RoutedEventArgs)
        closeFileDropDown()

        CollapseBar.Visibility = Visibility.Collapsed
        Toggle_OutBtn.Visibility = Visibility.Visible
        CalculationStack.Margin = New Thickness(40, 5, 5, 5)
    End Sub

    Private Sub OutOpen(sender As Object, e As RoutedEventArgs)
        closeFileDropDown()

        CollapseBar.Visibility = Visibility.Visible
        Toggle_OutBtn.Visibility = Visibility.Collapsed
        CalculationStack.Margin = New Thickness(5, 5, 5, 5)
    End Sub

    Private Sub closeBtn_Click(sender As Object, e As RoutedEventArgs)
        HorizantalStrapStack.Visibility = Visibility.Collapsed
        horiz_strap.Background = New SolidColorBrush(Colors.Transparent)

        AdditionalPostStack.Visibility = Visibility.Collapsed
        additionalPost.Background = New SolidColorBrush(Colors.Transparent)

        OpeningGridStack.Visibility = Visibility.Collapsed
        Opening.Background = New SolidColorBrush(Colors.Transparent)

        TopTrackStack.Visibility = Visibility.Collapsed
        top_track.Background = New SolidColorBrush(Colors.Transparent)

        IntersectionStack.Visibility = Visibility.Collapsed
        wall_inter.Background = New SolidColorBrush(Colors.Transparent)

        HorizontalBlockingStack.Visibility = Visibility.Collapsed
        horiz_block.Background = New SolidColorBrush(Colors.Transparent)

        EndPostStack.Visibility = Visibility.Collapsed
        end_post.Background = New SolidColorBrush(Colors.Transparent)

        closeFileDropDown()

    End Sub

    Private Sub closeCal_Click(sender As Object, e As RoutedEventArgs)

        Dim a = New BrushConverter()
        costCalculateBtn.Foreground = a.ConvertFrom(orangeColor)
        costCalculateBtn.Background = a.ConvertFrom(blueColor)

        CalculationStack.Visibility = Visibility.Collapsed
        drawingScales.Visibility = Visibility.Visible
        If IsFreeMode Then
            Return
        End If
        shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        closeFileDropDown()
    End Sub

    Private Sub saveBtnClick(sender As Object, e As RoutedEventArgs) Handles saveBtn.Click
        closeFileDropDown()
        shopDrawing.setUIWindow(Me)
        If panelHeight.Text = "" Then
            Return
        End If

        inputData.NumberofIntersectionWall = 0
        inputData.numberOfSlopes = 0

        inputData.NumberOfHeaders = 0

        inputData.topTrackData.Clear()
        Dim s1 = 0
        For index = 0 To topTrackrData.Count - 1
            If topTrackrData(index).xStart = 0 And topTrackrData(index).xEnd = 0 Then
                GoTo nextLevel
            End If

            inputData.topTrackData.Add(New TopTrackData() With {
            .ttSNo = s1 + 1,
            .height = topTrackrData(index).height,
            .slope = topTrackrData(index).slope,
            .xEnd = topTrackrData(index).xEnd,
            .xStart = topTrackrData(index).xStart
            })

            s1 = s1 + 1
            inputData.StartX(index) = topTrackrData(index).xStart
            inputData.EndX(index) = topTrackrData(index).xEnd
            inputData.HeightX(index) = topTrackrData(index).height
            inputData.SlopX(index) = topTrackrData(index).slope
            inputData.numberOfSlopes += 1
        Next
        topTrackrData.Add(New TopTrackData() With {
            .ttSNo = s1 + 1,
            .height = 0,
            .slope = 0,
            .xEnd = 0,
            .xStart = 0
            })

        topTrackrData(0).height = panelHeight.Text

nextLevel:

        skipCalculation = False
        For index = 0 To inputData.numberOfSlopes


            'If skipCalculation Then
            '    inputData.HeightX(index) = IIf(inputData.HeightX(index) > 0, inputData.HeightX(index), IIf(inputData.SlopX(index - 1) > 0, ((inputData.EndX(index - 1) - inputData.StartX(index - 1)) * inputData.SlopX(index - 1) + inputData.HeightX(index - 1)), inputData.HeightX(index - 1)))
            '    ''' inputData.HeightX(index) = inputData.HeightX(index - 1)
            '    topTrackrData(index).height = inputData.HeightX(index)
            '    Continue For
            'End If
            'If topTrackrData(index).height > 0 Then
            '    skipCalculation = True
            '    Continue For
            'End If


            If topTrackrData(index).height > 0 Then
                topTrackrData(index).height = topTrackrData(index).height
            Else
                inputData.HeightX(index) = (inputData.EndX(index - 1) - inputData.StartX(index - 1)) * inputData.SlopX(index - 1) + inputData.HeightX(index - 1)

                topTrackrData(index).height = inputData.HeightX(index)

            End If



            'ElseIf index = 0 Then
            '    topTrackrData(index).height = inputData.HeightX(0)
            'End If
        Next

        inputData.NumberOfSlope = inputData.numberOfSlopes
        wallLength.Text = inputData.EndX(inputData.NumberOfSlope - 1)
        inputData.wallHeight = panelHeight.Text
        inputData.wallLength = wallLength.Text
        inputData.numberOfOpenings = 0
        inputData.NumberOfInteriorPost = 0

        inputData.opening.Clear()
        Dim s2 = 0
        'reset opening boundary
        For i = 0 To inputData.LeftXBoundary.Length - 1
            inputData.LeftXBoundary(i) = 0
            inputData.RightXboundary(i) = 0
            inputData.SillOpening(i) = 0
            inputData.HeightOpening(i) = 0
            inputData.CenterOpening(i) = 0
            inputData.ShearPLate(i) = "False"
        Next
        For index = 0 To openingData.Count - 1
            If openingData(index).width = 0 And openingData(index).height = 0 Then
                GoTo other
            End If

            inputData.opening.Add(New Opening() With {
            .openSNo = s2 + 1,
            .centerX = openingData(index).centerX,
            .height = openingData(index).height,
            .sill = openingData(index).sill,
            .width = openingData(index).width,
            .headerWidth = openingData(index).headerWidth,
            .jambLeftin = openingData(index).jambLeftin,
            .jambRightIn = openingData(index).jambRightIn,
            .jambLeft = openingData(index).jambLeft,
            .jambRight = openingData(index).jambRight,
            .sherPlate = openingData(index).sherPlate,
            .headerDepth = openingData(index).headerDepth,
            .shersrc = openingData(index).shersrc
            })

            s2 += 1
            If openingData(index).sherPlate = False Then
                inputData.ShearPLate$(index) = "False"
            Else
                inputData.ShearPLate$(index) = "True"
            End If

            inputData.Header(index) = CType(openingData(index).headerDepth, Decimal)

            If inputData.Header(index) > 0 Then
                Dim val = headerClearence.Text
                If (val <> "") Then
                    inputData.Header(index) = inputData.Header(index) + Convert.ToDouble(headerClearence.Text)
                End If
            End If
            inputData.CenterOpening(index) = openingData(index).centerX
            inputData.WidthOpening(index) = openingData(index).width
            inputData.JambWidthLeft(index) = openingData(index).jambLeftin
            inputData.JambWidthRight(index) = openingData(index).jambRightIn
            inputData.LeftXBoundary(index) = inputData.CenterOpening(index) - 0.5 * inputData.WidthOpening(index)
            inputData.RightXboundary(index) = inputData.CenterOpening(index) + 0.5 * inputData.WidthOpening(index)
            inputData.HeightOpening(index) = openingData(index).height
            inputData.SillOpening(index) = openingData(index).sill
            inputData.numberOfOpenings += 1
        Next
other:
        inputData.WallLengthXMaximum = inputData.EndX(inputData.NumberOfSlope - 1)
        inputData.NumberOfOpening = inputData.numberOfOpenings + 1
        For s = 1 To inputData.NumberOfSlope
            ' for each slope calculate the height at StartX(s) and EndX(s) and find the maximum
            inputData.HeightXstart(s) = inputData.HeightX(s) ' defind HeightXStart(s) 'defind HeightXStart(s)
            If inputData.HeightXstart(s) > inputData.HeightYYMaximum Then inputData.HeightYYMaximum = inputData.HeightXstart(s)
            If s >= 1 Then
                inputData.HeightXEnd(s) = (inputData.EndX(s - 1) - inputData.StartX(s - 1)) * inputData.SlopX(s) + inputData.HeightX(s)
            End If
            If inputData.HeightXEnd(s) > inputData.HeightYYMaximum Then inputData.HeightYYMaximum = inputData.HeightXEnd(s)
        Next s
        inputData.HeightYYMaximum = inputData.HeightYYMaximum
        'save the data into
        inputData.NumberOfBlockingElement = 0
        inputData.horizantalBlocking.Clear()
        Dim s3 = 0
        For index = 1 To horizBlockData.Count
            Dim Cells = horizBlockData(index - 1)
            If IsDBNull(Cells.height) Or IsDBNull(Cells.depth) Then
                MessageBox.Show("Unvalid data Hrizontal data", "Error Information")
                Return
            End If
            ''save in a object
            inputData.horizantalBlocking.Add(New HorizantalBlocking() With {
                .hbSNo = s3 + 1,
            .depth = CType(Cells.depth, Double),
            .height = CType(Cells.height, Double),
            .type = CType(Cells.type, String)
            })
            s3 += 1
            inputData.BlockingHeight(index) = Cells.height
            inputData.WidthofHorizontalBlocing(index) = CType(Cells.depth, Double)
            inputData.BlockingType(index) = CType(Cells.type, String)
            inputData.NumberOfBlockingElement += 1
        Next
        'ReadData Horizontal 
        inputData.TotalNumberOfHorizontalStrips = 0
        inputData.horizantalStrap.Clear()
        Dim S4 = 0
        For i = 1 To horizStrapData.Count
            Dim Cells = horizStrapData(i - 1)

            If IsDBNull(Cells.locationHeight) Or IsDBNull(Cells.sizeDepth) Or IsDBNull(Cells.screwSpacing) Then
                MessageBox.Show("Unvalid data Hrizontal  strip data!", "Error Information")
                GoTo intersection
                Return
            End If

            If Cells.locationHeight = 0 Or Cells.sizeDepth = 0 Or Cells.screwSpacing = 0 Then
                GoTo intersection
            End If

            inputData.horizantalStrap.Add(New HorizantalStrap() With {
                .hsSNo = S4 + 1,
            .locationHeight = CType(Cells.locationHeight, Double),
            .sizeDepth = CType(Cells.sizeDepth, Double),
            .screwSpacing = CType(Cells.screwSpacing, String)
            })

            S4 = S4 + 1
            inputData.HorizontalStrapHeight(i) = CType(Cells.locationHeight, Double)
            inputData.HorizontalStripDepth(i) = CType(Cells.sizeDepth, Double)
            inputData.HorizontalStripScrew(i) = CType(Cells.screwSpacing, String)
            inputData.TotalNumberOfHorizontalStrips += 1
        Next

intersection:
        inputData.wallIntersection.Clear()
        Dim s5 = 0
        inputData.NumberofIntersectionWall = 0
        For i = 1 To wallintsData.Count
            Dim Cells = wallintsData(i - 1)

            If IsDBNull(Cells.wallWidth) Or IsDBNull(Cells.wallLocation) Then
                GoTo nextStep
                Return
            End If

            inputData.wallIntersection.Add(New WallIntersection() With {
                .wiSNo = s5 + 1,
            .wallLocation = CType(Cells.wallLocation, Double),
            .wallWidth = CType(Cells.wallWidth, Double)
            })
            inputData.NumberofIntersectionWall += 1
            s5 = s5 + 1
        Next
nextStep:
        inputData.additionalPost.Clear()

        inputData.NumberOfInteriorPost = 0
        Dim s6 = 0
        For i = 1 To addnPostData.Count
            Dim Cells = addnPostData(i - 1)
            If IsDBNull(Cells.postWidth) Or IsDBNull(Cells.postLocation) Or Cells.postWidth = 0 Or Cells.postLocation = 0 Then
                GoTo outside
                Return
            End If

            inputData.additionalPost.Add(New AdditionalPost() With {
                .apSNo = s6 + 1,
            .postLocation = CType(Cells.postLocation, Double),
            .postWidth = CType(Cells.postWidth, Double)
            })
            s6 = s6 + 1
            inputData.BackingStudsAx(i) = CType(Cells.postLocation, Double) 'Cells(33 + i, 2)
            inputData.WallBackingThickness(i) = CType(Cells.postWidth, Double) 'Cells(33 + i, 3)
            inputData.InteriorPostWidth(i) = inputData.WallBackingThickness(i)
            inputData.InterMetdiatPostAX(i) = inputData.BackingStudsAx(i)

            inputData.NumberOfInteriorPost += 1
        Next
outside:
        Dim t_emp As Double
        studSpacing.Text = IIf(Double.TryParse(studSpacing.Text, t_emp), studSpacing.Text, 0)
        studFlange.Text = If(Double.TryParse(studFlange.Text, t_emp), studFlange.Text, 0)
        studWeight.Text = IIf(Double.TryParse(studWeight.Text, t_emp), studWeight.Text, 0)
        studClearance.Text = IIf(Double.TryParse(studClearance.Text, t_emp), studClearance.Text, 0)
        studDepth.Text = IIf(Double.TryParse(studDepth.Text, t_emp), studDepth.Text, 0)
        trackWeight.Text = IIf(Double.TryParse(trackWeight.Text, t_emp), trackWeight.Text, 2)
        wall_name.Content = wallName.Text

        SaveStudData()
        file.Init(inputData, Me)
        shopDrawing.init(file)

        shopDrawing.MainForm.setInputData(inputData)

        file.SaveData()
    End Sub

    Private Sub SaveStudData()
        Dim num As Double = 0
        Dim bol As Boolean
        inputData.studsSpacing = If(Double.TryParse(studSpacing.Text, num) = False, 0, studSpacing.Text)

        inputData.studsFlange = If(Double.TryParse(studFlange.Text, num) = False, 0, studFlange.Text)
        inputData.studsDepth = If(Double.TryParse(studDepth.Text, num) = False, 0, studDepth.Text)

        inputData.StudsWidth = If(Double.TryParse(studFlange.Text, num) = False, 0, studFlange.Text)
        inputData.studClerance = If(Double.TryParse(studClearance.Text, num) = False, 0, studClearance.Text)

        inputData.labourCostPerHr = If(Double.TryParse(labourCostPerHr.Text, num) = False, 0, labourCostPerHr.Text)

        inputData.endOfPost = If(Double.TryParse(numberOfEndPost.Text, num) = False, 0, numberOfEndPost.Text)
        inputData.endPostBox = If(Boolean.TryParse(EndPostBox.Text, bol) = False, False, EndPostBox.Text)
        inputData.endPostWidth = If(Double.TryParse(endPostWeight.Text, num) = False, 0, endPostWeight.Text)
        inputData.trackFlange = If(Double.TryParse(trackFlangeInch.Text, num) = False, 0, trackFlangeInch.Text)
        inputData.companyName = companyName.Text

        inputData.wallName = wallName.Text

        inputData.laborCost = If(Double.TryParse(LabourCost.Text, num) = False, 0, LabourCost.Text)
        inputData.laborTime = If(Double.TryParse(labourTimeHr.Text, num) = False, 0, labourTimeHr.Text)

        inputData.screws = If(Double.TryParse(screws.Text, num) = False, 0, screws.Text)

        inputData.jambScrewSpa = If(Double.TryParse(jambPostScrewSpacing.Text, num) = False, 0, jambPostScrewSpacing.Text)

        inputData.totalCost = If(Double.TryParse(totalCost.Text, num) = False, 0, totalCost.Text)
        inputData.materialsCost = If(Double.TryParse(materialCost.Text, num) = False, 0, materialCost.Text)
        inputData.emailAddress = emailAddress.Text
        inputData.phoneNumber = phoneNumber.Text

        inputData.timeCut = If(Double.TryParse(cutPerMin.Text, num) = False, 0, cutPerMin.Text)
        inputData.timeSrc = If(Double.TryParse(screwInPerMin.Text, num) = False, 0, screwInPerMin.Text)
        inputData.headerScrewSpa = If(Double.TryParse(screwHeaderSpacing.Text, num) = False, 0, screwHeaderSpacing.Text)
        inputData.costScrew = If(Double.TryParse(costOfScrew.Text, num) = False, 0, costOfScrew.Text)
        inputData.costSteel = If(Double.TryParse(costOfSteel.Text, num) = False, 0, costOfSteel.Text)

        inputData.headerClerance = If(Double.TryParse(headerClearence.Text, num) = False, 0, headerClearence.Text)
        inputData.trackWeight = If(Double.TryParse(trackWeight.Text, num) = False, 0, trackWeight.Text)
        inputData.studWeight = If(Double.TryParse(studWeight.Text, num) = False, 0, studWeight.Text)

        inputData.screws = If(Double.TryParse(screws.Text, num) = False, 0, screws.Text)
        inputData.panelWeight = If(Double.TryParse(panelWeightLb.Text, num) = False, 0, panelWeightLb.Text)
        inputData.timeMem = If(Double.TryParse(timeMemInst.Text, num) = False, 0, timeMemInst.Text)

        inputData.steelThickness = steelThickneesGauge.Text
        inputData.projectName = projectName.Text

    End Sub


    Public pixelRatio As PixelRatios = New PixelRatios()
    Public Sub DrawTopTrack(factor)
        g.clearScreen()
        Dim data = inputData
        ' Define margin and axis length
        Dim margin As Integer = 50
        Dim axisLength As Integer = local_width - (margin * 2)
        'Draw X - Y axis

        If inputData.numberOfSlopes = 0 Then
            MessageBox.Show("Unvalid data top track data .Kindly reform toptrack data before procceding toward drawing.", "Error Information")
            topTrackBtn.Foreground = a.ConvertFrom(orangeColor)
            topTrackBtn.Background = a.ConvertFrom(blueColor)
            Return
        End If

        If shopDrawing.scaleDrawing <= 0.6 Then
            CanvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        Else
            CanvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        End If

        Dim wallHeight As Int32 = inputData.wallHeight

        Dim wallLength = inputData.EndX(inputData.NumberOfSlope - 1)

        Dim XstartLine As Integer
        Dim XEndline As Integer
        Dim YEndLine As Integer
        Dim YStartLine As Integer
        Dim WalLengthX As Integer
        Dim XMaximum As Integer
        Dim XRatio = (local_width - ((shopDrawing.scalePortion * local_width) / 100)) / data.WallLengthXMaximum
        Dim RatioX = XRatio * shopDrawing.scaleDrawing

        Dim HeightYYMaximum = data.HeightYYMaximum
        Dim RatioY = RatioX '(local_Height - ((20 * local_Height) / 100)) / HeightYYMaximum
        Dim OffsetX_X = margin

        If BaseWidth < 1 And BaseHeight < 1 Then
            BaseHeight = CanvasArea.ActualHeight
            BaseWidth = CanvasArea.ActualWidth
            BaseOffsetY = BaseHeight
        End If

        Dim ScaleRquired = 0
        Dim requiredScale As Double = 0
        If Double.TryParse(adjustFactorTxtBox.Text, ScaleRquired) Then
            requiredScale = ScaleRquired
        End If

        If requiredScale > 0 Then
            Dim pixelsForHeight As Double = shopDrawing.ScaleToDimenions(requiredScale)
            Dim requiredRatio As Double = pixelsForHeight / HeightYYMaximum
            Dim scalingRequired = requiredRatio / XRatio
            shopDrawing.scaleDrawing = scalingRequired
            RatioX = XRatio * shopDrawing.scaleDrawing
            RatioY = RatioX
            adjustFactorTxt.Value = shopDrawing.scaleDrawing
        End If
        If (shopDrawing.scaleDrawing * 100) < 65 And (requiredScale > 0) = False Then
            shopDrawing.scaleDrawing = 0.0
        End If

        Dim OffsetY_Y = local_Height - 100 '(local_Height * shopDrawing.scaleDrawing * (BaseWidth / BaseHeight) * 100) / 100 - (local_Height * shopDrawing.scaleDrawing * 60) / 100
        'Dim temp = CType(local_Height + local_Height * shopDrawing.scaleDrawing * ((BaseWidth / BaseHeight) * 100) / 100, Double)

        CanvasArea.Height = local_Height 'temp + (temp * 10) / 100
        CanvasArea.Width = local_width 'CType(local_width + (local_width * shopDrawing.scaleDrawing * 100) / 100, Double)

        scaleTxtLbl.Content = shopDrawing.ConvertScaleToPrintPage(HeightYYMaximum * RatioX, HeightYYMaximum).ToString("0.00") + " Inch"

        If (shopDrawing.scaleDrawing * 100) >= 65 Then

            CanvasScroll.ScrollToVerticalOffset(CanvasArea.Height / 2)

        End If
        pixelRatio.Xratio = RatioX
        pixelRatio.offsetY = OffsetY_Y
        pixelRatio.offsetX = OffsetX_X
        pixelRatio.MaxX = wallLength
        pixelRatio.MaxY = HeightYYMaximum
        pixelRatio.sizeScale = shopDrawing.scaleDrawing

        pixelRatio.scaleDrawing = requiredScale
        ''''''''''''''''''''''''''''kamel scale code'''''''''''''''''''''''''''''''''''''''''''''''
        'Dim dimScale = shopDrawing.DimensionScaling(data.HeightYYMaximum, data.WallLengthXMaximum)
        'Dim RatioX = dimScale.RatioX '(width - ((10 * width) / 100)) / data.WallLengthXMaximum
        ''data.HeightYYMaximum
        'Dim RatioY = dimScale.RatioY '(Height - ((20 * Height) / 100)) / HeightYYMaximum1
        'Dim OffsetX_X = dimScale.OffsetX '50
        'Dim OffsetY_Y = dimScale.OffsetY 'Height - 50
        'Dim RatioX = 750 / data.WallLengthXMaximum
        'Dim HeightYYMaximum = data.HeightYYMaximum
        'Dim RatioY = 200 / HeightYYMaximum

        Dim NumberOfSlope = data.NumberOfSlope

        Dim StartX = data.StartX
        Dim EndX = data.EndX
        Dim HeightX = topTrackrData

        For s = 0 To NumberOfSlope - 1

            XstartLine = StartX(s) * RatioX + OffsetX_X

            YStartLine = OffsetY_Y - HeightX(s).height * RatioY

            XEndline = EndX(s) * RatioX + OffsetX_X

            Dim prevX = HeightX(s + 1).height - (HeightX(s).height + (EndX(s) - StartX(s)) * inputData.SlopX(s))

            YEndLine = OffsetY_Y - (HeightX(s).height + (EndX(s) - StartX(s)) * inputData.SlopX(s)) * RatioX
            CanvasArea.Children.Add(g.DrawLine(System.Windows.Media.Brushes.DarkBlue, XEndline, YEndLine, XEndline, YEndLine - prevX * RatioX))

            CanvasArea.Children.Add(g.DrawLine(System.Windows.Media.Brushes.DarkBlue, XstartLine, YStartLine, XEndline, YEndLine))
        Next s

        ' draw the boundary of the wall at x=0 left wall boundery
        YStartLine = OffsetY_Y
        YEndLine = OffsetY_Y - HeightX(0).height * RatioY
        CanvasArea.Children.Add(g.DrawLine(System.Windows.Media.Brushes.DarkBlue, margin, YStartLine, margin, YEndLine))

        ' draw the boundary of the wall at y=0 at bottom
        XEndline = data.WallLengthXMaximum * RatioX + OffsetX_X

        CanvasArea.Children.Add(g.DrawLine(System.Windows.Media.Brushes.DarkBlue, margin, YStartLine, XEndline, YStartLine))

        YEndLine = OffsetY_Y - HeightX(NumberOfSlope).height * RatioY
        ' draw the boundary of the wall at x=WallLengthX-XMaximum

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkBlue, XEndline, YStartLine, XEndline, YEndLine))
        'Calculating pixel ratios. In all cases, Pixcel Ratio will be same for X Axis And Y axis.

        DrawDimensions(data, 50, RatioX, RatioY, OffsetY_Y - HeightX(0).height * RatioY, OffsetY_Y)

        '------------------------End Kamel code ------------------------|
        For i = 0 To data.numberOfOpenings - 1
            Dim recX = data.LeftXBoundary(i) * RatioX + OffsetX_X
            Dim rexY = OffsetY_Y - (data.HeightOpening(i) + data.SillOpening(i)) * RatioY
            Dim openingWidth = data.WidthOpening(i) * RatioX
            Dim openingHeight = data.HeightOpening(i) * RatioY
            g.DrawRectangle(System.Windows.Media.Brushes.DarkBlue, recX, rexY, openingWidth, openingHeight)
        Next i

        Dim leftBoundary(100) As Double
        Dim temp2 As Double
        Dim openSill(100) As Double

        For i = 0 To 100
            leftBoundary(i) = data.LeftXBoundary(i)
            openSill(i) = data.SillOpening(i)
        Next
        Dim temp
        For j As Integer = 1 To data.NumberOfOpening - 2
            For i As Integer = 0 To data.NumberOfOpening - 2
                If leftBoundary(i) >= leftBoundary(j) Then
                    temp = leftBoundary(i)
                    leftBoundary(i) = leftBoundary(j)
                    leftBoundary(j) = temp

                    temp2 = openSill(i)
                    openSill(i) = openSill(j)
                    openSill(j) = temp2
                End If
            Next
        Next

        For index As Integer = 0 To data.NumberOfOpening - 2
            DrawOpeningDimensions(RatioX, RatioY, data.LeftXBoundary(index), data.SillOpening(index), data.HeightOpening(index), data.WidthOpening(index), OffsetY_Y)
            OpeningLowerWidth((index + 1) * 5 * RatioY, RatioX, RatioY, leftBoundary(index), openSill(index), OffsetY_Y)
            If (data.SillOpening(index) = 0) Then
                Continue For
            End If
            OpeningSilHeight(index, RatioX, RatioY, data.LeftXBoundary(index), data.SillOpening(index), data.WidthOpening(index), OffsetY_Y)
        Next

    End Sub

#Region "Scales"
    Private Sub OpeningSilHeight(i As Integer, xRatio As Double, yRatio As Double, leftX As Double, sillHeight As Double, openWidth As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim openingWidth = margin + (openWidth * xRatio / (i + 1)) + (leftX * xRatio)
        Dim sill = offsetHeight - (sillHeight * yRatio)

        Dim x2 = 1.5 * xRatio
        Dim y2 = 1.5 * yRatio

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, openingWidth, offsetHeight, openingWidth, sill))
        g.DrawLabel(CanvasArea, sillHeight, 10, Brushes.Blue, openingWidth - 10, (offsetHeight + sill) / 2)

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, openingWidth + x2, offsetHeight - y2, openingWidth - x2, offsetHeight + y2))
        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, openingWidth + x2, sill - y2, openingWidth - x2, sill + y2))

    End Sub

    Private Sub OpeningLowerWidth(i As Integer, xRatio As Double, yRatio As Double, left As Double, sillHeight As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim offset_Height = OffsetY_Y + 4 * xRatio + i
        Dim LeftXBoundary = left * xRatio + margin
        Dim ylbl2 As String = left
        Dim xSpace = 1 * xRatio
        Dim ySpace = 1 * yRatio
        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, margin, offset_Height, LeftXBoundary, offset_Height)) ' opening - X-axis
        CanvasArea.Children.Add(g.DrawLine(Brushes.Pink, LeftXBoundary, offsetHeight, LeftXBoundary, offset_Height)) ' extra feature

        g.DrawLabel(CanvasArea, ylbl2, 10, Brushes.Blue, (LeftXBoundary / 2), offset_Height - 12)

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, margin, offset_Height - ySpace, margin, offset_Height + ySpace)) ' straight - left
        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, margin - xSpace, offset_Height + ySpace, margin + xSpace, offset_Height - ySpace)) ' left-slant

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, LeftXBoundary, offset_Height - ySpace, LeftXBoundary, offset_Height + ySpace)) 'straight - right
        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, LeftXBoundary - xSpace, offset_Height + ySpace, LeftXBoundary + xSpace, offset_Height - ySpace)) ' right- slant

    End Sub

    Private Sub DrawOpeningDimensions(xRatio As Double, yRatio As Double, leftX As Double, sillheight As Double, openHeight As Double, openWidth As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim open_Height = openHeight * yRatio
        Dim open_Width = openWidth * xRatio
        Dim LeftXBoundary = leftX * xRatio + margin

        Dim sill = offsetHeight - (sillheight * yRatio)
        Dim Xlbl As String = openHeight
        Dim ylbl As String = openWidth

        Dim xSpace = 1.5 * xRatio
        Dim ySpace = 1.5 * yRatio

        Dim openingWidth = margin + (open_Width / 4) + (leftX * xRatio)
        Dim openingHeight = offsetHeight - (((open_Height / 2)) + (sillheight * yRatio))
        Dim yend = offsetHeight - ((open_Height) + (sillheight * yRatio))

        CanvasArea.Children.Add(g.DrawLine(Brushes.Purple, LeftXBoundary, openingHeight, LeftXBoundary + open_Width, openingHeight)) ' opening - width
        g.DrawLabel(CanvasArea, ylbl, 10, Brushes.Blue, LeftXBoundary + (open_Width / 2), openingHeight - 10)

        CanvasArea.Children.Add(g.DrawLine(Brushes.DarkCyan, openingWidth, sill, openingWidth, yend))  ' Opening - height
        g.DrawLabel(CanvasArea, Xlbl, 10, Brushes.Blue, openingWidth - 10, sill - (open_Height / 2.5))

        '  draw crosses
        CanvasArea.Children.Add(g.DrawLine(Brushes.Purple, LeftXBoundary - xSpace, openingHeight + ySpace, LeftXBoundary + xSpace, openingHeight - ySpace))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Purple, LeftXBoundary + open_Width - xSpace, openingHeight + ySpace, LeftXBoundary + open_Width + xSpace, openingHeight - ySpace))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Purple, openingWidth - xSpace, sill + ySpace, openingWidth + xSpace, sill - ySpace))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Purple, openingWidth - xSpace, yend + ySpace, openingWidth + xSpace, yend - ySpace))

    End Sub

    Private Sub DrawDimensions(data As InputData, margin As Double, xRatio As Double, yRatio As Double, yend As Double, OffsetY_Y As Double)
        Dim offsetHeight = OffsetY_Y
        Dim y = offsetHeight - (data.wallHeight * yRatio) / 2
        Dim x = data.WallLengthXMaximum * xRatio + margin
        Dim Xlbl As String = data.WallLengthXMaximum
        Dim ylbl As String = data.wallHeight
        Dim xSpace = 2 * xRatio
        Dim ySpace = 2 * yRatio
        Dim yPoint = OffsetY_Y + 4 * xRatio

        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin, yPoint, x, yPoint)) ' X-axis
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin / 2, offsetHeight, margin / 2, yend)) ' Y-axis

        g.DrawLabel(CanvasArea, Xlbl, 12, Brushes.Black, x / 2, yPoint - 14)
        g.DrawLabel(CanvasArea, ylbl, 12, Brushes.Black, margin / 2 - 10, y)

        '  draw crosses
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin, yPoint - ySpace, margin, yPoint + ySpace))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin - xSpace, yPoint + ySpace, margin + xSpace, yPoint - ySpace))


        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, x, yPoint - ySpace, x, yPoint + ySpace))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, x - xSpace, yPoint + ySpace, x + xSpace, yPoint - ySpace))

        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, (margin / 2) - xSpace, offsetHeight, (margin / 2) + xSpace, offsetHeight))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, (margin / 2) - xSpace, offsetHeight + ySpace, (margin / 2) + xSpace, offsetHeight - ySpace))

        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, (margin / 2) - xSpace, yend, (margin / 2) + xSpace, yend))
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, (margin / 2) - xSpace, yend + ySpace, (margin / 2) + xSpace, yend - ySpace))

    End Sub

    Private Sub DrawScales(xMax As Double, yMax As Double, height As Double, margin As Double, axisLength As Double, xRatio As Double, yRatio As Double)

        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin, height - margin, margin + axisLength, height - margin)) ' X-axis
        CanvasArea.Children.Add(g.DrawLine(Brushes.Red, margin, height - margin, margin, margin)) ' Y-axis
        Dim XIntervals = Math.Round(xMax / 10)
        Dim YIntervals = Math.Round(yMax / 10)
        'Draw ticks On X-axis, 10 will be length of that tick on the axis line
        For i As Integer = 1 To 10
            Dim x As Integer = margin + i * (XIntervals * xRatio)
            CanvasArea.Children.Add(g.DrawLine(Brushes.Black, x, height - margin - 10, x, height - margin + 10))
            'Add label by each tick
            Dim lbl As String = i * XIntervals
            g.DrawLabel(CanvasArea, lbl, 12, Brushes.Black, x - 10, height - margin + 5)
        Next

        'Draw ticks on Y-axis
        For i As Integer = 1 To 5
            Dim y As Integer = margin + i * (YIntervals * yRatio)

            CanvasArea.Children.Add(g.DrawLine(Brushes.Black, margin - 10, height - y, margin + 10, height - y))
            ''Add Label by each tick
            Dim lbl As String = i * YIntervals
            g.DrawLabel(CanvasArea, lbl, 12, Brushes.Black, margin - 35, height - y - 12)
        Next
    End Sub
#End Region

    Private Sub SaveWallBtnClick(sender As Object, e As RoutedEventArgs) Handles saveWallBtn.Click
        closeFileDropDown()
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        Me.shopDrawing.SaveDxfFile()
    End Sub

    Private Sub SavedDatainGrids_Click(sender As Object, e As RoutedEventArgs)
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        topTrackBtn.RaiseEvent(New RoutedEventArgs(topTrackBtn.ClickEvent))
        If IsFreeMode = False Then
            shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        End If
        closeBtn_Click(sender, e)
    End Sub

    Private Sub newBtn_Click(sender As Object, e As RoutedEventArgs)

        closeBtn_Click(sender, e)
        g.clearScreen()

        Me.g = New Graphics()
        Me.g.setCanvas(CanvasArea)

        shopDrawing = New ShopDrawingForm()
        shopDrawing.setUIWindow(Me)
        inputData = New InputData()
        file = New FileHandling()
        file.Init(inputData, Me)
        Me.DataContext = Me

        defaultButtons()
        drawingScales.Visibility = Visibility.Visible
        elementDetailStatck.Visibility = Visibility.Collapsed
        CalculationStack.Visibility = Visibility.Collapsed
        pdfWebViewer.Visibility = Visibility.Collapsed

        topTrackrData = New ObservableCollection(Of TopTrackData)()
        openingData = New ObservableCollection(Of Opening)()
        wallintsData = New ObservableCollection(Of WallIntersection)()
        addnPostData = New ObservableCollection(Of AdditionalPost)()
        horizStrapData = New ObservableCollection(Of HorizantalStrap)()
        horizBlockData = New ObservableCollection(Of HorizantalBlocking)()
        costCalcData = New ObservableCollection(Of CostCalculationsData)()
        elementDetailData = New ObservableCollection(Of ElementDetail)()
        DataGridViewTopTrack.ItemsSource = topTrackrData
        OpeningGrid.ItemsSource = openingData
        DataGridViewIntersection.ItemsSource = wallintsData
        DataGridViewAdditionalPost.ItemsSource = addnPostData
        DataGridViewHorizantalStrap.ItemsSource = horizStrapData
        DataGridViewHorizontalBlocking.ItemsSource = horizBlockData
        DataGridViewCostCalculations.ItemsSource = costCalcData
        dataGridElementDetail.ItemsSource = elementDetailData

        wallName.Text = String.Empty
        studWeight.Text = String.Empty
        trackWeight.Text = String.Empty
        EndPostBox.Text = String.Empty
        numberOfEndPost.Text = String.Empty
        endPostWeight.Text = String.Empty
        studDepth.Text = String.Empty
        studFlange.Text = String.Empty
        studSpacing.Text = 24
        headerClearence.Text = 0.125
        studClearance.Text = 0.125
        steelThickneesGauge.Text = String.Empty
        trackFlangeInch.Text = String.Empty
        costOfSteel.Text = String.Empty
        costOfScrew.Text = String.Empty
        screwInPerMin.Text = String.Empty
        cutPerMin.Text = String.Empty
        timeMemInst.Text = String.Empty
        screwHeaderSpacing.Text = String.Empty
        jambPostScrewSpacing.Text = String.Empty
        labourCostPerHr.Text = String.Empty
        companyName.Text = String.Empty
        emailAddress.Text = String.Empty
        phoneNumber.Text = String.Empty
        projectName.Text = String.Empty
        panelWeightLb.Text = String.Empty
        screws.Text = String.Empty
        labourTimeHr.Text = String.Empty
        materialCost.Text = String.Empty
        LabourCost.Text = String.Empty
        totalCost.Text = String.Empty
        wall_name.Content = String.Empty
        wallLength.Text = String.Empty
        panelHeight.Text = String.Empty
        topTrackSlopes.Text = String.Empty

        adjustFactorTxt.Value = 0.1
        scaleTxtLbl.Content = "0 Inch"
    End Sub

    Private Sub OpenBtn_Click(sender As Object, e As RoutedEventArgs)
        closeBtn_Click(sender, e)

        file.LoadFile()

        elementDetailStatck.Visibility = Visibility.Collapsed
        drawingScales.Visibility = Visibility.Visible

        CalculationStack.Visibility = Visibility.Collapsed
        CanvasArea.Visibility = Visibility.Visible
        Dim a = New BrushConverter()
        topTrackBtn.Foreground = a.ConvertFrom(blueColor)
        topTrackBtn.Background = a.ConvertFrom(orangeColor)

        shopDrawingBtn.Foreground = a.ConvertFrom(orangeColor)
        shopDrawingBtn.Background = a.ConvertFrom(blueColor)

        costCalculateBtn.Foreground = a.ConvertFrom(orangeColor)
        costCalculateBtn.Background = a.ConvertFrom(blueColor)
        If IsFreeMode = True Then
            FreeTrialItems()
        End If
    End Sub
    Private Async Sub LogOutBtn_Click(sender As Object, e As RoutedEventArgs)

        Me.LoginGrid.Visibility = Visibility.Visible

        'Me.userEmail.Text = ""
        'Me.userPassword.Password = ""
        'Me.pdKeyTxt.Text = ""
        'Me.emailLoginMsg.Content = ""
        'Me.PasswordLoginMsg.Content = ""
        'Me.pdKeyMsg.Content = ""

        Dim dataStore = New DataStoreAccess()

        Await dataStore.dataStore.UpdateData(Meta.IsLogout, "LOGOUT")


        dataStore.SyncData()
        Me.MainGrid.Visibility = Visibility.Collapsed
        entryMsgGrid.Visibility = Visibility.Collapsed
    End Sub

    Private Sub TextBox_KeyUp(sender As Object, e As Input.KeyEventArgs)

        'If e.Key = Key.Down Then
        '    Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Next
        '    ' MoveFocus takes a TraveralReqest as its argument.
        '    Dim request As New TraversalRequest(focusDirection)
        '    ' Gets the element with keyboard focus.
        '    Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)
        '    ' Change keyboard focus.
        '    If elementWithFocus IsNot Nothing Then
        '        elementWithFocus.MoveFocus(request)
        '    End If
        '    ' Set Handled to True to prevent further processing of the down arrow key event.
        '    e.Handled = True
        'End If
        'If e.Key = Key.Up Then
        '    Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Previous
        '    ' MoveFocus takes a TraveralReqest as its argument.
        '    Dim request As New TraversalRequest(focusDirection)
        '    ' Gets the element with keyboard focus.
        '    Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)
        '    ' Change keyboard focus.
        '    If elementWithFocus IsNot Nothing Then
        '        elementWithFocus.MoveFocus(request)
        '    End If
        '    ' Set Handled to True to prevent further processing of the down arrow key event.
        '    e.Handled = True
        'End If
        'closeFileDropDown()

    End Sub

    Private Sub ComboBox_KeyUp(sender As Object, e As Input.KeyEventArgs)

        'If e.Key = Key.Down Then

        '    Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Next

        '    ' MoveFocus takes a TraveralReqest as its argument.
        '    Dim request As New TraversalRequest(focusDirection)

        '    ' Gets the element with keyboard focus.
        '    Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)

        '    ' Change keyboard focus.
        '    If elementWithFocus IsNot Nothing Then
        '        elementWithFocus.MoveFocus(request)
        '    End If

        '    ' Set Handled to True to prevent further processing of the down arrow key event.
        '    e.Handled = True
        'End If

        'If e.Key = Key.Up Then

        '    Dim focusDirection As FocusNavigationDirection = FocusNavigationDirection.Previous

        '    ' MoveFocus takes a TraveralReqest as its argument.
        '    Dim request As New TraversalRequest(focusDirection)

        '    ' Gets the element with keyboard focus.
        '    Dim elementWithFocus As UIElement = TryCast(Keyboard.FocusedElement, UIElement)

        '    ' Change keyboard focus.
        '    If elementWithFocus IsNot Nothing Then
        '        elementWithFocus.MoveFocus(request)
        '    End If

        '    ' Set Handled to True to prevent further processing of the down arrow key event.
        '    e.Handled = True
        'End If
        'closeFileDropDown()

    End Sub

    Private Sub topTrackBtn_Click(sender As Object, e As RoutedEventArgs) Handles topTrackBtn.Click
        closeFileDropDown()

        If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Or TryCast(costCalculateBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Or TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString1 Then
            topTrackBtn.Background = a.ConvertFrom(orangeColor)
            topTrackBtn.Foreground = a.ConvertFrom(blueColor)

            costCalculateBtn.Foreground = a.ConvertFrom(orangeColor)
            costCalculateBtn.Background = a.ConvertFrom(blueColor)

            shopDrawingBtn.Foreground = a.ConvertFrom(orangeColor)
            shopDrawingBtn.Background = a.ConvertFrom(blueColor)
        End If
        wall_name.Visibility = Visibility.Visible
        drawingScales.Visibility = Visibility.Visible

        pdfWebViewer.Visibility = Visibility.Collapsed
        elementDetailStatck.Visibility = Visibility.Collapsed
        CalculationStack.Visibility = Visibility.Collapsed
        CanvasArea.Visibility = Visibility.Visible

        CanvasArea.Children.Clear()
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        DrawTopTrack(1)
    End Sub

    Private Sub shopDrawingBtn_Click(sender As Object, e As RoutedEventArgs) Handles shopDrawingBtn.Click
        closeFileDropDown()

        If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Or TryCast(costCalculateBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Or TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString1 Then
            topTrackBtn.Foreground = a.ConvertFrom(orangeColor)
            topTrackBtn.Background = a.ConvertFrom(blueColor)

            costCalculateBtn.Foreground = a.ConvertFrom(orangeColor)
            costCalculateBtn.Background = a.ConvertFrom(blueColor)

            shopDrawingBtn.Foreground = a.ConvertFrom(blueColor)
            shopDrawingBtn.Background = a.ConvertFrom(orangeColor)
        End If

        elementDetailStatck.Visibility = Visibility.Visible
        wall_name.Visibility = Visibility.Visible
        drawingScales.Visibility = Visibility.Visible

        pdfWebViewer.Visibility = Visibility.Collapsed
        CalculationStack.Visibility = Visibility.Collapsed
        CanvasArea.Visibility = Visibility.Visible
        If IsFreeMode Then
            Return
        End If
        CanvasArea.Children.Clear()
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        shopDrawing.ShopDrawingForm_Paint(g, inputData, local_Height, local_width)
        skipCalculation = False
    End Sub

    Private Sub costCalculateBtn_Click(sender As Object, e As RoutedEventArgs) Handles costCalculateBtn.Click
        closeFileDropDown()

        If CalculationStack.Visibility = Visibility.Visible Then
            closeCal_Click(sender, e)
        Else
            pdfWebViewer.Visibility = Visibility.Collapsed
            CalculationStack.Visibility = Visibility.Visible
            CanvasArea.Visibility = Visibility.Collapsed
            elementDetailStatck.Visibility = Visibility.Collapsed
            wall_name.Visibility = Visibility.Collapsed

            costCalculateBtn.Foreground = a.ConvertFrom(blueColor) 'light
            costCalculateBtn.Background = a.ConvertFrom(orangeColor) 'dark

            topTrackBtn.Foreground = a.ConvertFrom(orangeColor)
            topTrackBtn.Background = a.ConvertFrom(blueColor)

            shopDrawingBtn.Foreground = a.ConvertFrom(orangeColor)
            shopDrawingBtn.Background = a.ConvertFrom(blueColor)

            CanvasArea.Children.Clear()
            saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))

            shopDrawing.ShopDrawingForm_Paint(g, inputData, local_Height, local_width)
            shopDrawing.CostEstimation()

            drawingScales.Visibility = Visibility.Collapsed
            CalculationStack.Visibility = Visibility.Visible

            Dim HeightOfcalculationStack = DirectCast(CalculationStack.Parent, System.Windows.FrameworkElement).ActualHeight
            DataGridViewCostCalculations.Height = HeightOfcalculationStack - 40

        End If
    End Sub

#Region "New Item "
    Private Sub DataGridViewTopTrack_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New TopTrackData() With {
            .ttSNo = sender.items.count,
            .slope = 0.25}
    End Sub

    Private Sub OpeningGrid_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New Opening() With {
           .openSNo = sender.items.count,
           .jambLeft = "1",
           .jambRight = "1",
           .jambLeftin = "1.625",
           .jambRightIn = "1.625",
           .headerDepth = "0",
           .sherPlate = False}
    End Sub

    Private Sub DataGridViewAdditionalPost_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New AdditionalPost() With {
          .apSNo = sender.items.count}
    End Sub

    Private Sub DataGridViewIntersection_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New WallIntersection() With {
          .wiSNo = sender.items.count}
    End Sub

    Private Sub DataGridViewHorizantalStrap_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New HorizantalStrap() With {
          .hsSNo = sender.items.count}
    End Sub

    Private Sub DataGridViewHorizontalBlocking_AddingNewItem(sender As Object, e As AddingNewItemEventArgs)
        e.NewItem = New HorizantalBlocking() With {
          .hbSNo = sender.items.count}
    End Sub

#End Region

    Private Sub saveAsBtn_Click(sender As Object, e As RoutedEventArgs)
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        file.SaveAsData()
        closeFileDropDown()
    End Sub

    Private Sub Window_MouseCapture(sender As Object, e As MouseButtonEventArgs)
        If sender.datacontext IsNot fileDropDown Then
            closeFileDropDown()
        End If
        If sender.datacontext Is FileBtn Then
            closeFileDropDown()
        End If
        If sender.DataContext IsNot innerStack Then
            closeElementDetail()
        End If
    End Sub

    Private Sub Window_LostMouseCapture(sender As Object, e As Input.MouseEventArgs)
        If sender.datacontext IsNot fileDropDown And sender.datacontext Is FileBtn Then
            closeFileDropDown()
        End If
        If sender.datacontext Is FileBtn Then
            closeFileDropDown()
        End If
        If TypeOf sender.datacontext Is TextBox Then
            Dim textBox As TextBox = DirectCast(sender, TextBox)
            textBox.SelectAll()
        End If
    End Sub

    Private Sub defaultButtons()
        Dim a = New BrushConverter()
        costCalculateBtn.Background = a.ConvertFrom(blueColor)
        costCalculateBtn.Foreground = a.ConvertFrom(orangeColor)

        topTrackBtn.Background = a.ConvertFrom(blueColor)
        topTrackBtn.Foreground = a.ConvertFrom(orangeColor)

        shopDrawingBtn.Background = a.ConvertFrom(blueColor)
        shopDrawingBtn.Foreground = a.ConvertFrom(orangeColor)
    End Sub

    Private Sub dropDown_Checked(sender As Object, e As RoutedEventArgs)
        closeFileDropDown()
        Dim a = New Animation.DoubleAnimation()
        innerStack.Measure(New Size(ElementDetailStack.MaxWidth, ElementDetailStack.MaxHeight))
        a = New Animation.DoubleAnimation(innerStack.DesiredSize.Height, duration)
        ElementDetailStack.BeginAnimation(HeightProperty, a)
    End Sub

    Private Sub dropDown_Unchecked(sender As Object, e As RoutedEventArgs)
        closeFileDropDown()

        Dim a = New Animation.DoubleAnimation()
        a = New Animation.DoubleAnimation(0, duration)
        ElementDetailStack.BeginAnimation(HeightProperty, a)
    End Sub

    Private Sub printBtn_Click(sender As Object, e As RoutedEventArgs) Handles printBtn.Click
        closeFileDropDown()
        Try
            exportPdfBtn_Click(sender, e)
            pdfWebViewer.Visibility = Visibility.Visible
            drawingScales.Visibility = Visibility.Collapsed
            pdfWebViewer.Navigate(file.exportPDFFile)
            defaultButtons()
        Catch ex As Exception
            pdfWebViewer.Visibility = Visibility.Collapsed
            drawingScales.Visibility = Visibility.Visible
            Return
        End Try
    End Sub

    Private Sub exportPdfBtn_Click(sender As Object, e As RoutedEventArgs) Handles exportPdfBtn.Click
        Dim IsFreeTrial As Boolean = True

        closeFileDropDown()
        CanvasArea.Children.Clear()
        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))
        CanvasArea.Visibility = Visibility.Visible
        DrawTopTrack(1)
        Dim mainFilePath = file.ExportPDF()
        If mainFilePath Is Nothing Then
            Return
        End If

        Dim inputPath = System.IO.Path.GetTempFileName() + ".xps"

        Dim pdfPath = PDFExport.PDFExport.ConvertCanvasToXPS(CanvasArea, inputPath)

        Dim listOfPath = New List(Of String) From {
           pdfPath
        }

        If IsFreeMode Then
            PDFExport.PDFExport.StackPdfPages(mainFilePath, listOfPath)
            Return
        End If
        g.clearScreen()

        shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))

        Dim shopDrawingPath = System.IO.Path.GetTempFileName() + ".xps"
        listOfPath.Add(PDFExport.PDFExport.ConvertCanvasToXPS(CanvasArea, shopDrawingPath))
        shopDrawing.CostEstimation()

        PDFExport.PDFExport.StackPdfPages(mainFilePath, listOfPath)
        For I = 0 To listOfPath.Count - 1
            FileIO.FileSystem.DeleteFile(listOfPath(I))
        Next

        PDFExport.PDFExport.AddMetaDataInfo(mainFilePath, scaleTxtLbl.Content, shopDrawing.MainForm.inputData.wallName, IsFreeTrial)
        PDFExport.PDFExport.AddElementDetails(elementDetailData, mainFilePath, shopDrawing.MainForm.inputData.wallName)
        PDFExport.PDFExport.AddWallCostEstimation(costCalcData, mainFilePath, shopDrawing.MainForm.inputData.wallName)
        PDFExport.PDFExport.AddCostCalaculation(costCalcData, mainFilePath, shopDrawing.MainForm.inputData.wallName)
    End Sub

    Private Sub btnRemove_Checked(sender As Object, e As RoutedEventArgs)

        closeFileDropDown()
        If Me.TopTrackStack.Visibility = Visibility.Visible Then
            Try

                Dim selectedItems = DataGridViewTopTrack.SelectedItems.Cast(Of TopTrackData)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then

                    For Each item In selectedItems
                        topTrackrData.Remove(item)
                        inputData.topTrackData.Remove(item)
                    Next

                    For index = 0 To topTrackrData.Count - 1
                        topTrackrData(index).ttSNo = index + 1
                    Next

                Else
                    Return
                End If

            Catch
                Return
            End Try
        End If

        If Me.OpeningGridStack.Visibility = Visibility.Visible Then
            Try

                Dim selectedItems = OpeningGrid.SelectedItems.Cast(Of Opening)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then

                    For Each item In selectedItems
                        openingData.Remove(item)
                        inputData.opening.Remove(item)
                    Next

                    For index = 0 To openingData.Count - 1
                        openingData(index).openSNo = index + 1
                    Next
                Else
                    Return
                End If
            Catch
                Return
            End Try
        End If

        If Me.IntersectionStack.Visibility = Visibility.Visible Then
            Try
                Dim selectedItems = DataGridViewIntersection.SelectedItems.Cast(Of WallIntersection)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then

                    For Each item In selectedItems
                        wallintsData.Remove(item)
                        inputData.wallIntersection.Remove(item)
                    Next

                    For index = 0 To wallintsData.Count - 1
                        wallintsData(index).wiSNo = index + 1
                    Next
                Else
                    Return
                End If
            Catch
                Return
            End Try
        End If

        If Me.AdditionalPostStack.Visibility = Visibility.Visible Then
            Try
                Dim selectedItems = DataGridViewAdditionalPost.SelectedItems.Cast(Of AdditionalPost)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then

                    For Each item In selectedItems
                        addnPostData.Remove(item)
                        inputData.additionalPost.Remove(item)
                    Next

                    For index = 0 To addnPostData.Count - 1
                        addnPostData(index).apSNo = index + 1
                    Next
                Else
                    Return
                End If
            Catch
                Return
            End Try
        End If

        If Me.HorizantalStrapStack.Visibility = Visibility.Visible Then
            Try

                Dim selectedItems = DataGridViewHorizantalStrap.SelectedItems.Cast(Of HorizantalStrap)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then

                    For Each item In selectedItems
                        horizStrapData.Remove(item)
                        inputData.horizantalStrap.Remove(item)
                    Next

                    For index = 0 To horizStrapData.Count - 1
                        horizStrapData(index).hsSNo = index + 1
                    Next
                Else
                    Return
                End If
            Catch
                Return
            End Try
        End If

        If Me.HorizontalBlockingStack.Visibility = Visibility.Visible Then
            Try
                Dim selectedItems = DataGridViewHorizontalBlocking.SelectedItems.Cast(Of HorizantalBlocking)().ToList()
                If selectedItems.Count = 0 Then
                    Return
                End If
                Dim result = MessageBox.Show("Are you sure you want to delete this row?", "Delete Alert", CType(MessageBoxButton.YesNo, MessageBoxButtons))
                If result = Forms.DialogResult.Yes Then
                    For Each item In selectedItems
                        horizBlockData.Remove(item)
                        inputData.horizantalBlocking.Remove(item)
                    Next

                    For index = 0 To horizBlockData.Count - 1
                        horizBlockData(index).hbSNo = index + 1
                    Next
                Else
                    Return
                End If
            Catch
                Return
            End Try
        End If
        If shopDrawing.MainForm IsNot Nothing Then
            shopDrawing.MainForm.setInputData(inputData)
        End If

        closeElementDetail()
    End Sub

    Private Sub DataGridRow_Loaded(sender As Object, e As RoutedEventArgs)
        If TypeOf sender Is DataGridRow Then
            Dim row As DataGridRow = DirectCast(sender, DataGridRow)
            row.Background = Brushes.Red
        End If
    End Sub
#Region "Focus"
    Private Sub DataGridViewTopTrack_GotFocus(sender As Object, e As RoutedEventArgs)
        DataGridViewTopTrack.BeginEdit()
    End Sub

    Private Sub OpeningGrid_GotFocus(sender As Object, e As RoutedEventArgs)
        OpeningGrid.BeginEdit()
    End Sub

    Private Sub DataGridViewAdditionalPost_GotFocus(sender As Object, e As RoutedEventArgs)
        DataGridViewAdditionalPost.BeginEdit()
    End Sub

    Private Sub DataGridViewIntersection_GotFocus(sender As Object, e As RoutedEventArgs)
        DataGridViewIntersection.BeginEdit()
    End Sub

    Private Sub DataGridViewHorizantalStrap_GotFocus(sender As Object, e As RoutedEventArgs)
        DataGridViewHorizantalStrap.BeginEdit()
    End Sub

    Private Sub DataGridViewHorizontalBlocking_GotFocus(sender As Object, e As RoutedEventArgs)
        DataGridViewHorizontalBlocking.BeginEdit()
    End Sub

    Private Sub adjustFactorTxt_ValueChanged(sender As Object, e As RoutedPropertyChangedEventArgs(Of Double))
        Dim slider As Slider = DirectCast(sender, Slider)
        Dim inputValue As Double = slider.Value
        Try
            If Not String.IsNullOrEmpty(inputValue) AndAlso IsNumeric(inputValue) Then
                Dim numericValue As Double = Convert.ToDouble(inputValue)
                If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
                    shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
                End If

                If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
                    topTrackBtn.RaiseEvent(New RoutedEventArgs(topTrackBtn.ClickEvent))
                End If
            Else
                Return
            End If
        Catch ex As Exception
            Return
        End Try
    End Sub

    Private Sub adjustFactorTxtBox_TextInput(sender As Object, e As Input.KeyEventArgs)

        saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))

        Dim v = CType(sender, TextBox).Text
        If e.Key <> Key.Enter Then
            Return
        End If
        If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        End If

        If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            topTrackBtn.RaiseEvent(New RoutedEventArgs(topTrackBtn.ClickEvent))
        End If
    End Sub

#End Region
    Private Sub okBtn_Click(sender As Object, e As RoutedEventArgs)
        MsgGrid.Visibility = Visibility.Collapsed
    End Sub

    Private Sub plusMinusBtn_Click(sender As Object, e As RoutedEventArgs)
        Try
            closeFileDropDown()
            Dim canva
            If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
                canva = "shop_draw"
            End If

            If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
                canva = "top_track"
            End If

            Dim path = createPdf(canva)
            If path = "error" Then
                Return
            End If
            pdf_WebViewer.Navigate(path)
            MainGrid.Visibility = Visibility.Collapsed
            pdf_WebViewerGrid.Visibility = Visibility.Visible
        Catch ex As Exception
            Return
        End Try
    End Sub

    Private Function createPdf(Canva As String) As String
        Try
            Dim inputPath = System.IO.Path.GetTempFileName() + ".xps"
            Dim pdfPath
            If Canva = "top_track" Then

                CanvasArea.Children.Clear()

                saveBtn.RaiseEvent(New RoutedEventArgs(saveBtn.ClickEvent))

                CanvasArea.Visibility = Visibility.Visible
                DrawTopTrack(1)
                pdfPath = PDFExport.PDFExport.ConvertCanvasToXPS(CanvasArea, inputPath)

            End If

            If Canva = "shop_draw" Then
                CanvasArea.Children.Clear()
                CanvasArea.Visibility = Visibility.Visible
                shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
                pdfPath = PDFExport.PDFExport.ConvertCanvasToXPS(CanvasArea, inputPath)

            End If


            Return pdfPath
        Catch ex As Exception
            Return "error"
        End Try

    End Function

    Private Sub closingPdf_click(sender As Object, e As RoutedEventArgs)
        MainGrid.Visibility = Visibility.Visible
        pdf_WebViewerGrid.Visibility = Visibility.Collapsed

        If TryCast(shopDrawingBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            shopDrawingBtn.RaiseEvent(New RoutedEventArgs(shopDrawingBtn.ClickEvent))
        End If

        If TryCast(topTrackBtn.Background, SolidColorBrush)?.Color.ToString() = colorString Then
            topTrackBtn.RaiseEvent(New RoutedEventArgs(topTrackBtn.ClickEvent))
        End If
    End Sub

    Private Sub Guid_navigate(sender As Object, e As RoutedEventArgs)
        Process.Start(New ProcessStartInfo("https://smartbuildanalytics.ginilytics.org/help/"))
        e.Handled = True
    End Sub

    Private Sub CreateProject_Click(sender As Object, e As RoutedEventArgs)

    End Sub
End Class
