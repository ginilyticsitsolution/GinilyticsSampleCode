Imports System.IO
Imports Model.Modal

Public Class ScalDimension
    Public units As Double
    Public OffsetX As Double
    Public OffsetY As Double
    Public RatioX As Double
    Public RatioY As Double
End Class
Public Class ShopDrawingForm

    Dim n As Integer = 0
    Dim ElementNumber = 0
    Dim LowerLeftJambBoundary
    Dim UpperLeftJambBoundary
    Dim LowerRightJambBoundary
    Dim UpperRightJambBoundary
    Dim TotalNumberOfElement
    Dim numberOfInterMidPost = 0
    Dim TotalNumberOfmember = 0
    Dim Height As Integer
    Dim width As Integer
    Dim VerticalElement As Integer = 0
    Dim WallHeightX As Double

    Public uiWindow As MainCasaWindow
    Public MainForm As Main
    Public fileDxf As FileHandling
    ReadOnly Brushes As Brushes
    Private PixelRatio As Double = 1

    Public topbktrackData As BacktrackDataTopTrackData = New BacktrackDataTopTrackData()
    'interpost 

    Public Sub init(file As FileHandling)
        Me.MainForm = New Main()
        Me.fileDxf = file
    End Sub
    Public BaseHeight As Double
    Public BaseWidth As Double
    Public BaseOffsetY As Double

    Public Sub setUIWindow(window As MainCasaWindow)
        Me.uiWindow = window
        Me.scaleDrawing = IIf(Me.uiWindow.adjustFactorTxt.Value = 0, 0.6, Me.uiWindow.adjustFactorTxt.Value.ToString("0.00"))
        Me.uiWindow.adjustmentTickLbl.Content = CType(Double.Parse(Me.scaleDrawing).ToString("0.00") * 100, Int32)

    End Sub


    Public Function FormatePrecision(num As Object) As String
        Dim temp As Double = 0

        If Double.TryParse(num, temp) Then
            Return temp.ToString("0.00")
        End If
        Return "0"
    End Function

    Public Sub FormateData(inputData As InputData)
        'Formate data
        Me.uiWindow.costCalcData.Clear()
        Dim counter = 1
        Dim data = inputData

        Me.uiWindow.costCalcData.Clear()

        For i = 0 To verticalElement_temp

            If data.CXX(i) = 0 Or data.BYY(i) = 0 Then
                Continue For
            End If
            Dim sorted = New CostCalculationsData()
            If data.VERTICALMEMBER(i) = "Bottom Track" Or data.VERTICALMEMBER(i) = "H.Blocking" Or data.VERTICALMEMBER(i) = "Horizontal Strap" Or data.VERTICALMEMBER(i) = "Shear Plate Left" Or data.VERTICALMEMBER(i) = "Shear Plate Right" Or data.VERTICALMEMBER(i) = "Header" Or data.VERTICALMEMBER(i) = "Opening Bottom Track" Or data.VERTICALMEMBER(i) = "Opening Top Track" Then
                sorted.lengthInch = IIf((data.CXX(i) - data.AXX(i)) < 0, (data.AXX(i) - data.CXX(i)).ToString("0.00"), (data.CXX(i) - data.AXX(i)).ToString("0.00"))
            ElseIf data.VERTICALMEMBER(i) = "Top Track" Then
                sorted.lengthInch = (Math.Sqrt((data.CXX(i) - data.AXX(i)) * (data.CXX(i) - data.AXX(i)) + (data.CYY(i) - data.AYY(i)) * (data.CYY(i) - data.AYY(i)))).ToString("0.00")
            Else
                sorted.lengthInch = IIf((data.BYY(i) - data.AYY(i)) < 0, (data.AYY(i) - data.BYY(i)).ToString("0.00"), (data.BYY(i) - data.AYY(i)).ToString("0.00"))
            End If

            sorted.member = data.VERTICALMEMBER(i)
            sorted.element = counter
            counter += 1
            Me.uiWindow.costCalcData.Add(sorted)
        Next

    End Sub

    Public Function getSteelThickness() As Double
        Dim t As String = Me.uiWindow.steelThickneesGauge.Text

        If Me.uiWindow.steelThickneesGauge.Text = "" Or Me.uiWindow.steelThickneesGauge.Text = "0" Then
            Return 0
        End If

        If IsNothing(t) Then
            Return 0
        End If
        Return t.Split("|")(1)
    End Function


    Dim totalTrackWeight As Double = 0

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="index"></param>
    ''' <param name="MemeberWeightPerFT"></param>
    ''' <param name="numberOfScrew"></param>
    ''' <param name="memeberType"></param>
    Public Sub PrintCost(index As Integer, MemeberWeightPerFT As Double, numberOfScrew As Integer, memeberType As String, numberofCut As Integer)
        Dim data = MainForm.inputData
        Dim MemberWeight = MemeberWeightPerFT * Me.uiWindow.costCalcData(index).lengthInch / 12

        If memeberType = "stud" Then
            totalStudWeight += MemberWeight
        End If

        If memeberType = "Top Track" Or memeberType = "Bottom Track" Then

            numberOfScrew = 0
        End If


        Dim MemberCost = data.costSteel * MemberWeight
        Dim CostOfScrew = numberOfScrew * data.costScrew
        Dim TotalCostOfMaterail = MemberCost + CostOfScrew
        Dim TotalTimeOfScrewing = (data.timeSrc * numberOfScrew) / 60 'minutes to hours
        Dim screwLabourTimeCost = TotalTimeOfScrewing * data.labourCostPerHr
        Dim TimeOfCut = (numberofCut * data.timeCut) / 60
        Dim CostOfCut = TimeOfCut * data.labourCostPerHr

        Dim TimeOfInstallation = data.timeMem / 60

        Dim CostOfInstallation = TimeOfInstallation * data.labourCostPerHr

        Dim CostMaterials = MemberCost + CostOfScrew
        Dim TimeOfLabor = TimeOfInstallation + TotalTimeOfScrewing + TimeOfCut
        Dim CostOfLabor = CostOfInstallation + screwLabourTimeCost + CostOfCut
        Dim TotalCostLabourMaterails = CostOfLabor + CostMaterials



        Me.uiWindow.costCalcData(index).totalWeight = MemberWeight.ToString("0.00")
        Me.uiWindow.costCalcData(index).cutsNumber = numberofCut.ToString("0.00")
        Me.uiWindow.costCalcData(index).installTimeHr = TimeOfInstallation.ToString("0.00")
        Me.uiWindow.costCalcData(index).costMember = MemberCost.ToString("0.00")
        Me.uiWindow.costCalcData(index).installCost = CostOfInstallation.ToString("0.00")
        Me.uiWindow.costCalcData(index).materialScrewCost = CostOfScrew.ToString("0.00")
        Me.uiWindow.costCalcData(index).totalCost = TotalCostLabourMaterails.ToString("0.00")
        Me.uiWindow.costCalcData(index).totalMaterialCost = CostMaterials.ToString("0.00")
        Me.uiWindow.costCalcData(index).timeToScrewHr = TotalTimeOfScrewing.ToString("0.00")
        Me.uiWindow.costCalcData(index).screw = numberOfScrew

        Me.uiWindow.costCalcData(index).CutCost = CostOfCut.ToString("0.00")
        Me.uiWindow.costCalcData(index).costLabScre = screwLabourTimeCost.ToString("0.00")
        Me.uiWindow.costCalcData(index).labourCost = CostOfLabor.ToString("0.00")
        Me.uiWindow.costCalcData(index).labourTotalHr = TimeOfLabor.ToString("0.00")
        Me.uiWindow.costCalcData(index).CutTimeHr = TimeOfCut.ToString("0.00")

        totalCost += TotalCostLabourMaterails.ToString("0.00")
        totalLabourHr += TimeOfLabor.ToString("0.00")
        totalScrew += numberOfScrew
        totalWeight += MemberWeight.ToString("0.00")
        totalLabourCost += CostOfLabor.ToString("0.00")
        totalMaterialCost += CostMaterials.ToString("0.00")
        totalNumberOfCuts += numberofCut
        totalTimeCutHour += TimeOfCut.ToString("0.00")
        totalInstallationTime += TimeOfInstallation.ToString("0.00")
        totalCutCost += CostOfCut.ToString("0.00")
        totalScrewLabourCost += screwLabourTimeCost.ToString("0.00")
        totalScrewMaterialCost += CostOfScrew.ToString("0.00")
        totalCostMemeber += MemberCost.ToString("0.00")
        totalTimeOfScrew += TotalTimeOfScrewing.ToString("0.00")
        totalInstallationCost += CostOfInstallation.ToString("0.00")
        totallength += Me.uiWindow.costCalcData(index).lengthInch.ToString("0.00")

    End Sub

    Dim totalCost = 0
    Dim totalLabourHr = 0
    Dim totalLabourCost = 0
    Dim totalWeight = 0
    Dim totalScrew = 0
    Dim totalMaterialCost = 0
    Dim totalStudWeight = 0

    Dim totalNumberOfCuts = 0
    Dim totalTimeCutHour = 0
    Dim totalInstallationTime = 0
    Dim totalCutCost = 0
    Dim totalScrewLabourCost = 0
    Dim totalScrewMaterialCost = 0
    Dim totalCostMemeber = 0
    Dim totalTimeOfScrew = 0
    Dim totalInstallationCost = 0
    Dim totallength = 0


    'cost estimation
    Public Sub AssignNumericLableToElements(RatioX As Double, RatioY As Double, OffsetX_X As Double, OffsetY_Y As Double, draw As Boolean)
        Dim data = MainForm.inputData
        For k = 0 To 20000 - 1
            data.AXX(k) = 0
            data.AYY(k) = 0
            data.BXX(k) = 0
            data.BYY(k) = 0
            data.CXX(k) = 0
            data.CYY(k) = 0
            data.DXX(k) = 0
            data.DYY(k) = 0
        Next

        Dim pos = 0

        data.VERTICALMEMBER(1000) = String.Empty

        For position = 0 To verticalElement_temp + n
            If data.StudsPointBY(position) = 0 Or data.StudsPointCX(position) = 0 Or data.AXX(position) > data.wallLength Then
                Continue For
            End If

            data.AXX(pos) = data.StudsPointAX(position)
            data.BXX(pos) = data.StudsPointBX(position)
            data.CXX(pos) = data.StudsPointCX(position)
            data.DXX(pos) = data.StudsPointDX(position)
            data.AYY(pos) = data.StudsPointAY(position)
            data.BYY(pos) = data.StudsPointBY(position)
            data.CYY(pos) = data.StudsPointCY(position)
            data.DYY(pos) = data.StudsPointDY(position)
            data.VERTICALMEMBER(pos) = "Studs"
            pos += 1
        Next

        verticalElement_temp = pos + 1
        pos = 0
        For position = 0 To (2 * data.NumberOfOpening)
            If data.JambAX(position) = 0 And data.JambBY(position) = 0 Or data.JambCX(position) = 0 Or data.JambAX(position) > data.wallLength Then
                Continue For
            End If
            data.AXX(verticalElement_temp + pos) = data.JambAX(position)
            data.BXX(verticalElement_temp + pos) = data.JambBX(position)
            data.CXX(verticalElement_temp + pos) = data.JambCX(position)
            data.DXX(verticalElement_temp + pos) = data.JambDX(position)
            data.AYY(verticalElement_temp + pos) = data.JambAY(position)
            data.BYY(verticalElement_temp + pos) = data.JambBY(position)
            data.CYY(verticalElement_temp + pos) = data.JambCY(position)
            data.DYY(verticalElement_temp + pos) = data.JambDY(position)
            data.VERTICALMEMBER(verticalElement_temp + pos) = "Jambs"
            pos += 1
        Next

        verticalElement_temp = verticalElement_temp + pos
        pos = 0
        If data.endPostBox Then
            For position = 1 To 2 * data.endOfPost
                data.AXX(verticalElement_temp + position) = data.EndPostStXA(position)
                data.BXX(verticalElement_temp + position) = data.EndPostStXB(position)
                data.CXX(verticalElement_temp + position) = data.EndPostStXC(position)
                data.DXX(verticalElement_temp + position) = data.EndPostStXD(position)
                data.AYY(verticalElement_temp + position) = data.EndPostStYA(position)
                data.BYY(verticalElement_temp + position) = data.EndPostStYB(position)
                data.CYY(verticalElement_temp + position) = data.EndPostStYC(position)
                data.DYY(verticalElement_temp + position) = data.EndPostStYD(position)
                data.EndPostLength(position) = data.EndPostStYD(position) - data.EndPostStYC(position)
                data.VERTICALMEMBER(verticalElement_temp + position) = "End Post"
                pos += 1
            Next
            verticalElement_temp = 2 * data.endOfPost + verticalElement_temp + pos + 1
        End If

        pos = 0
        For position = 1 To data.NumberOfInteriorPost
            data.AXX(verticalElement_temp + position) = data.InterMetdiatPostAX(position)
            data.BXX(verticalElement_temp + position) = data.InterMetdiatPostBX(position)
            data.CXX(verticalElement_temp + position) = data.InterMetdiatPostCX(position)
            data.DXX(verticalElement_temp + position) = data.InterMetdiatPostDX(position)
            data.AYY(verticalElement_temp + position) = data.InterMetdiatPostAY(position)
            data.BYY(verticalElement_temp + position) = data.InterMetdiatPostBY(position)
            data.CYY(verticalElement_temp + position) = data.InterMetdiatPostCY(position)
            data.DYY(verticalElement_temp + position) = data.InterMetdiatPostDY(position)
            data.InterMediatPostLength(position) = data.InterMetdiatPostDY(position) - data.InterMetdiatPostCY(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = "Interior Post"
            pos += 1
        Next

        verticalElement_temp = data.NumberOfInteriorPost + verticalElement_temp

        For position = 1 To numberofStrips
            data.AXX(verticalElement_temp + position) = data.HorizontalStrapHeightAx(position)
            data.BXX(verticalElement_temp + position) = data.HorizontalStrapHeightBx(position)
            data.CXX(verticalElement_temp + position) = data.HorizontalStrapHeightCx(position)
            data.DXX(verticalElement_temp + position) = data.HorizontalStrapHeightDx(position)
            data.AYY(verticalElement_temp + position) = data.HorizontalStrapHeightAy(position)
            data.BYY(verticalElement_temp + position) = data.HorizontalStrapHeightBy(position)
            data.CYY(verticalElement_temp + position) = data.HorizontalStrapHeightCy(position)
            data.DYY(verticalElement_temp + position) = data.HorizontalStrapHeightDy(position)
            data.HorizontalStrapLength(position) = data.HorizontalStrapHeightDy(position) - data.HorizontalStrapHeightCy(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = "Horizontal Strap"
        Next

        verticalElement_temp += numberofStrips

        For position = 1 To data.NumberOfHeaders
            data.AXX(verticalElement_temp + position) = data.OpeningAX(position)
            data.BXX(verticalElement_temp + position) = data.OpeningBX(position)
            data.CXX(verticalElement_temp + position) = data.OpeningCX(position)
            data.DXX(verticalElement_temp + position) = data.OpeningDX(position)
            data.AYY(verticalElement_temp + position) = data.OpeningAY(position)
            data.BYY(verticalElement_temp + position) = data.OpeningBY(position)
            data.CYY(verticalElement_temp + position) = data.OpeningCY(position)
            data.DYY(verticalElement_temp + position) = data.OpeningDY(position)
            data.HeaderLength(position) = data.OpeningCX(position) - data.OpeningAX(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = "Header"
        Next

        verticalElement_temp += data.NumberOfHeaders + 1

        For position = 0 To hblockingNumber - 1
            data.AXX(verticalElement_temp + position) = data.HorizontalBlockingAX(position)
            data.BXX(verticalElement_temp + position) = data.HorizontalBlockingBX(position)
            data.CXX(verticalElement_temp + position) = data.HorizontalBlockingCX(position)
            data.DXX(verticalElement_temp + position) = data.HorizontalBlockingDX(position)
            data.AYY(verticalElement_temp + position) = data.HorizontalBlockingAY(position)
            data.BYY(verticalElement_temp + position) = data.HorizontalBlockingBY(position)
            data.CYY(verticalElement_temp + position) = data.HorizontalBlockingCY(position)
            data.DYY(verticalElement_temp + position) = data.HorizontalBlockingDY(position)

            data.HorizontalBlockingLength(position) = data.HorizontalBlockingDY(position) - data.HorizontalBlockingCY(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = IIf(data.HorizontalBlockingMemeberType(position) = "Horizontal", "H.Blocking", "V.Blocking")
        Next

        verticalElement_temp += (hblockingNumber + 1)

        For i = 0 To 2 * (data.NumberofIntersectionWall - 1) - 1
            data.AXX(verticalElement_temp + i + 1) = data.IntersectionWallAX(i)
            data.BXX(verticalElement_temp + i + 1) = data.IntersectionWallBX(i)
            data.CXX(verticalElement_temp + i + 1) = data.IntersectionWallcX(i)
            data.DXX(verticalElement_temp + i + 1) = data.IntersectionWalldX(i)
            data.AYY(verticalElement_temp + i + 1) = data.IntersectionWallAY(i)
            data.BYY(verticalElement_temp + i + 1) = data.IntersectionWallBy(i)
            data.CYY(verticalElement_temp + i + 1) = data.IntersectionWallcy(i)
            data.DYY(verticalElement_temp + i + 1) = data.IntersectionWalldy(i)
            data.VERTICALMEMBER(verticalElement_temp + i + 1) = "Intersection Wall"
        Next

        verticalElement_temp += 2 * (data.NumberofIntersectionWall - 1) + 1

        For i = 0 To data.numberOfOpenings
            data.AXX(verticalElement_temp + i) = data.ShearPlateLeftAX(i)
            data.AYY(verticalElement_temp + i) = data.ShearPlateLeftAY(i)
            data.BXX(verticalElement_temp + i) = data.ShearPlateLeftBX(i)
            data.BYY(verticalElement_temp + i) = data.ShearPlateLeftBY(i)
            data.CXX(verticalElement_temp + i) = data.ShearPlateLeftCX(i)
            data.CYY(verticalElement_temp + i) = data.ShearPlateLeftCY(i)
            data.DXX(verticalElement_temp + i) = data.ShearPlateLeftDX(i)
            data.DYY(verticalElement_temp + i) = data.ShearPlateLeftDY(i)
            data.VERTICALMEMBER(verticalElement_temp + i) = "Shear Plate Left"
        Next

        verticalElement_temp += data.numberOfOpenings

        For i = 0 To data.numberOfOpenings
            data.AXX(verticalElement_temp + i) = data.ShearPlateRightAX(i)
            data.AYY(verticalElement_temp + i) = data.ShearPlateRightAY(i)
            data.BXX(verticalElement_temp + i) = data.ShearPlateRightBX(i)
            data.BYY(verticalElement_temp + i) = data.ShearPlateRightBY(i)
            data.CXX(verticalElement_temp + i) = data.ShearPlateRightCX(i)
            data.CYY(verticalElement_temp + i) = data.ShearPlateRightCY(i)
            data.DXX(verticalElement_temp + i) = data.ShearPlateRightDX(i)
            data.DYY(verticalElement_temp + i) = data.ShearPlateRightDY(i)
            data.VERTICALMEMBER(verticalElement_temp + i) = "Shear Plate Right"
        Next
        verticalElement_temp += data.numberOfOpenings

        data.AXX(verticalElement_temp) = topbktrackData.BottomTrackAX
        data.BXX(verticalElement_temp) = topbktrackData.BottomTrackBX
        data.CXX(verticalElement_temp) = topbktrackData.BottomTrackCX
        data.DXX(verticalElement_temp) = topbktrackData.BottomTrackDX
        data.AYY(verticalElement_temp) = topbktrackData.BottomTrackAY
        data.BYY(verticalElement_temp) = topbktrackData.BottomTrackBY
        data.CYY(verticalElement_temp) = topbktrackData.BottomTrackCY
        data.DYY(verticalElement_temp) = topbktrackData.BottomTrackDY
        data.VERTICALMEMBER(verticalElement_temp) = "Bottom Track"
        verticalElement_temp += 1

        For POSITION = 0 To data.NumberOfSlope - 1
            data.AXX(verticalElement_temp + POSITION) = topbktrackData.AX(POSITION)
            data.BXX(verticalElement_temp + POSITION) = topbktrackData.BX(POSITION)
            data.CXX(verticalElement_temp + POSITION) = topbktrackData.CX(POSITION)
            data.DXX(verticalElement_temp + POSITION) = topbktrackData.DX(POSITION)
            data.AYY(verticalElement_temp + POSITION) = topbktrackData.AY(POSITION)
            data.BYY(verticalElement_temp + POSITION) = topbktrackData.BY(POSITION)
            data.CYY(verticalElement_temp + POSITION) = topbktrackData.CY(POSITION)
            data.DYY(verticalElement_temp + POSITION) = topbktrackData.DY(POSITION)
            data.VERTICALMEMBER(verticalElement_temp + POSITION) = "Top Track"
        Next

        verticalElement_temp += (data.NumberOfSlope)


        For position = 1 To data.numberOfOpenings
            data.AXX(verticalElement_temp + position) = data.TopTrackAXO(position)
            data.AYY(verticalElement_temp + position) = data.TopTrackAYO(position)
            data.BXX(verticalElement_temp + position) = data.TopTrackBXO(position)
            data.BYY(verticalElement_temp + position) = data.TopTrackBYO(position)
            data.CXX(verticalElement_temp + position) = data.TopTrackCXO(position)
            data.CYY(verticalElement_temp + position) = data.TopTrackCYO(position)
            data.DXX(verticalElement_temp + position) = data.TopTrackDXO(position)
            data.DYY(verticalElement_temp + position) = data.TopTrackDYO(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = "Opening Top Track"

        Next
        verticalElement_temp = verticalElement_temp + data.numberOfOpenings


        For position = 1 To data.numberOfOpenings
            data.AXX(verticalElement_temp + position) = data.BottomTrackAXO(position)
            data.AYY(verticalElement_temp + position) = data.BottomTrackAYO(position)
            data.BXX(verticalElement_temp + position) = data.BottomTrackBXO(position)
            data.BYY(verticalElement_temp + position) = data.BottomTrackBYO(position)
            data.CXX(verticalElement_temp + position) = data.BottomTrackCXO(position)
            data.CYY(verticalElement_temp + position) = data.BottomTrackCYO(position)
            data.DXX(verticalElement_temp + position) = data.BottomTrackDXO(position)
            data.DYY(verticalElement_temp + position) = data.BottomTrackDYO(position)
            data.VERTICALMEMBER(verticalElement_temp + position) = "Opening Bottom Track"

        Next
        verticalElement_temp = verticalElement_temp + data.numberOfOpenings


        For i = 0 To verticalElement_temp - 1
            For k = i + 1 To verticalElement_temp
                If (data.AXX(i) > data.AXX(k)) Then
                    Dim AX = data.AXX(i)
                    Dim AY = data.AYY(i)
                    Dim BX = data.BXX(i)
                    Dim BY = data.BYY(i)
                    Dim CX = data.CXX(i)
                    Dim CY = data.CYY(i)
                    Dim DX = data.DXX(i)
                    Dim DY = data.DYY(i)
                    Dim member = data.VERTICALMEMBER(i)

                    data.AXX(i) = data.AXX(k)
                    data.AYY(i) = data.AYY(k)
                    data.BXX(i) = data.BXX(k)
                    data.BYY(i) = data.BYY(k)
                    data.CXX(i) = data.CXX(k)
                    data.CYY(i) = data.CYY(k)
                    data.DXX(i) = data.DXX(k)
                    data.DYY(i) = data.DYY(k)
                    data.VERTICALMEMBER(i) = data.VERTICALMEMBER(k)

                    data.AXX(k) = AX
                    data.AYY(k) = AY
                    data.BXX(k) = BX
                    data.BYY(k) = BY
                    data.CXX(k) = CX
                    data.CYY(k) = CY
                    data.DXX(k) = DX
                    data.DYY(k) = DY

                    data.VERTICALMEMBER(k) = member
                End If
            Next
        Next
        If draw = False Then
            Return
        End If
        Dim count = 1
        Dim labelSize = 12
        Me.uiWindow.elementDetailData.Clear()
        Dim hb = 0
        For i = 0 To verticalElement_temp
            If data.CXX(i) = 0 Then
                Continue For
            End If

            If data.VERTICALMEMBER(i) = "Horizontal Strap" Or data.VERTICALMEMBER(i) = "Shear Plate Left" Or data.VERTICALMEMBER(i) = "Shear Plate Right" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))
            ElseIf data.VERTICALMEMBER(i) = "Header" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))
            ElseIf data.VERTICALMEMBER(i) = "H.Blocking" Then

                hb += 1
                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))
            ElseIf data.VERTICALMEMBER(i) = "Top Track" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))
            ElseIf data.VERTICALMEMBER(i) = "Opening Top Track" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize + labelSize * 0.25)))

            ElseIf data.VERTICALMEMBER(i) = "Opening Bottom Track" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))

            ElseIf data.VERTICALMEMBER(i) = "Bottom Track" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX, OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY + (labelSize / 2)))
            ElseIf data.VERTICALMEMBER(i) = "End Post" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX - (10 / 2), OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY))
            ElseIf data.VERTICALMEMBER(i) = "Intersection Wall" Then

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.Black, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX - (10 / 2), OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY))
            Else

                uiWindow.g.DrawLabel(count.ToString(), labelSize, Brushes.DarkGreen, OffsetX_X + ((data.AXX(i) + data.DXX(i) + data.BXX(i) + data.CXX(i)) / 4) * RatioX - (10 / 2), OffsetY_Y - (((data.AYY(i) + data.BYY(i) + data.CYY(i) + data.DYY(i)) / 4) * RatioY))
            End If

            If data.VERTICALMEMBER(i) = "Bottom Track" Or data.VERTICALMEMBER(i) = "H.Blocking" Or data.VERTICALMEMBER(i) = "V.Blocking" Or data.VERTICALMEMBER(i) = "Horizontal Strap" Or data.VERTICALMEMBER(i) = "Shear Plate Left" Or data.VERTICALMEMBER(i) = "Shear Plate Right" Or data.VERTICALMEMBER(i) = "Header" Or data.VERTICALMEMBER(i) = "Opening Top Track" Or data.VERTICALMEMBER(i) = "Opening Bottom Track" Then
                Me.uiWindow.elementDetailData.Add(
                New ElementDetail() With {
                                    .yAxis = data.AYY(i),
                                    .edSNo = count,
                                    .elementName = data.VERTICALMEMBER(i),
                                    .elementWidth = IIf((data.BYY(i) - data.AYY(i)) < 0, (data.AYY(i) - data.BYY(i)).ToString("0.00"), (data.BYY(i) - data.AYY(i)).ToString("0.00")),
                                    .elementHeight = IIf((data.CXX(i) - data.AXX(i)) < 0, (data.AXX(i) - data.CXX(i)).ToString("0.00"), (data.CXX(i) - data.AXX(i)).ToString("0.00"))
                                     })
            ElseIf data.VERTICALMEMBER(i) = "Top Track" Then
                Me.uiWindow.elementDetailData.Add(
                New ElementDetail() With {
                  .yAxis = data.AYY(i),
                                    .edSNo = count,
                                    .elementName = data.VERTICALMEMBER(i),
                                    .elementWidth = IIf((data.BYY(i) - data.AYY(i)) < 0, (data.AYY(i) - data.BYY(i)).ToString("0.00"), (data.BYY(i) - data.AYY(i)).ToString("0.00")),
                                    .elementHeight = (Math.Sqrt((data.CXX(i) - data.AXX(i)) * (data.CXX(i) - data.AXX(i)) + (data.CYY(i) - data.AYY(i)) * (data.CYY(i) - data.AYY(i)))).ToString("0.00")
                                     })
            Else
                Me.uiWindow.elementDetailData.Add(
                New ElementDetail() With {
                  .yAxis = data.AYY(i),
                                    .edSNo = count,
                                    .elementName = data.VERTICALMEMBER(i),
                                    .elementHeight = IIf((data.BYY(i) - data.AYY(i)) < 0, (data.AYY(i) - data.BYY(i)).ToString("0.00"), (data.BYY(i) - data.AYY(i)).ToString("0.00")),
                                    .elementWidth = IIf((data.CXX(i) - data.AXX(i)) < 0, (data.AXX(i) - data.CXX(i)).ToString("0.00"), (data.CXX(i) - data.AXX(i)).ToString("0.00"))
                                     })
            End If
            count += 1
        Next
    End Sub
    Dim BoxTrackWeight As Double = 0
    Dim BoxStudWeight As Double = 0
    Dim BoxWeight As Double = 0
    Public Sub BoxWeightCalcualtion(BoxWidth As Double, steelThickness As Double, trackFlange As Double, boxDepth As Double, studsWidth As Double)
        BoxTrackWeight = (BoxWidth + 2 * trackFlange) * (steelThickness / 144) * 490
        BoxStudWeight = ((boxDepth + 2 * studsWidth + 0.75 * 2) * steelThickness / 144) * 490
        BoxWeight = (BoxTrackWeight + BoxStudWeight) * 2
    End Sub
    Public Sub CostEstimation()
        Dim data = Me.MainForm.inputData
        FormateData(data)
        totalTrackWeight = 0
        totalCost = 0
        totalLabourHr = 0
        totalWeight = 0
        totalScrew = 0
        totalLabourCost = 0
        totalMaterialCost = 0
        totalStudWeight = 0

        totalNumberOfCuts = 0
        totalTimeCutHour = 0
        totalInstallationTime = 0
        totalCutCost = 0
        totalScrewLabourCost = 0
        totalScrewMaterialCost = 0
        totalCostMemeber = 0
        totalTimeOfScrew = 0
        totalInstallationCost = 0
        totallength = 0

        Dim studDepth As Double = data.studsDepth
        Dim studFleng As Double = data.studsFlange
        Dim StudWeight As Double = 0
        Dim steelThickness As Double = getSteelThickness()
        Dim MemeberWeightPerFT As Double = 0
        Dim StudsWidth As Double = data.StudsWidth
        Dim trackFelnge As Double = data.trackFlange
        Dim BoxWidth As Double = data.studsFlange
        Dim BoxDepth As Double = data.studsDepth
        Dim studWeightPerft As Double = 0
        Dim posShPlate = 0
        Dim HorizontalScrewSpacing = 0
        'sort shearscrew

        BoxTrackWeight = (BoxDepth + 2 * trackFelnge) * (steelThickness / 144) * 490
        BoxStudWeight = ((BoxDepth + 2 * StudsWidth + 0.75 * 2) * steelThickness / 144) * 490
        BoxWeight = (BoxTrackWeight + BoxStudWeight) * 2


        'studs
        Dim NumberOfScrew = 4
        GoTo calculateSudWeight
calculateCost:
        Me.uiWindow.studWeight.Text = Double.Parse(studWeightPerft).ToString("0.00")
        Me.uiWindow.trackWeight.Text = Double.Parse(BoxTrackWeight).ToString("0.00")

        For index As Integer = 0 To Me.uiWindow.elementDetailData.Count - 1
            Dim mType = Me.uiWindow.elementDetailData(index).elementName
            If mType = "End Post" Or mType = "Interior Post" Or mType = "Jambs" Or mType = "Horizontal Strap" Or mType = "H.Blocking" Or mType = "Top Track" Or mType = "Bottom Track" Or mType = "Header" Or mType = "Opening Top Track" Or mType = "Opening Bottom Track" Or mType = "Intersection Wall" Then
                If data.jambScrewSpa = 0 Then
                    MessageBox.Show("Show jamb screw spacing cannot be 0", "Error")
                    Return
                End If
                BoxWidth = uiWindow.elementDetailData(index).elementWidth

                Select Case uiWindow.elementDetailData(index).elementName
                    Case "Horizontal Strap"
                        Dim screwSpacing = 0
                        Dim position = 0
                        While uiWindow.horizStrapData(position).locationHeight <> Me.uiWindow.elementDetailData(index).yAxis
                            position += 1
                        End While
                        screwSpacing = uiWindow.horizStrapData(position).screwSpacing

                        Dim len = uiWindow.elementDetailData(index).elementHeight
                        NumberOfScrew = len / screwSpacing
                        Dim sizeDepth = uiWindow.elementDetailData(index).elementWidth
                        Dim MemberWeightPerFT = ((steelThickness * sizeDepth / 144) * 490)
                        'MemberWeightPerFT = sizeDepth
                        PrintCost(index, MemberWeightPerFT, NumberOfScrew, uiWindow.elementDetailData(index).elementName, 2)
                    Case "Jambs"
                        If BoxWidth > data.studsFlange.ToString("0.00") Then
                            Dim length = uiWindow.elementDetailData(index).elementHeight
                            BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)
                            Dim MemberWeightPerFT = BoxWeight
                            Dim NumberOfCut = 8
                            NumberOfScrew = (length / data.jambScrewSpa) * 4
                            PrintCost(index, MemberWeightPerFT, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                        Else
                            GoTo studCalculate
                        End If
                    Case "Intersection Wall"
                        If BoxWidth > data.studsFlange Then
                            Dim length = uiWindow.elementDetailData(index).elementHeight
                            BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)
                            Dim MemberWeightPerFT = BoxStudWeight
                            Dim NumberOfCut = 2
                            NumberOfScrew = 4
                            PrintCost(index, MemberWeightPerFT, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                        Else
                            GoTo studCalculate
                        End If
                    Case "Interior Post"
                        If BoxWidth > data.studsFlange.ToString("0.00") Then
                            Dim length = uiWindow.elementDetailData(index).elementHeight
                            BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)
                            Dim MemberWeightPerFT = BoxWeight
                            Dim NumberOfCut = 8
                            NumberOfScrew = ((length / 12) / data.jambScrewSpa) * 4
                            PrintCost(index, MemberWeightPerFT, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                        Else
                            GoTo studCalculate
                        End If
                    Case "End Post"
                        If BoxWidth > data.studsFlange.ToString("0.00") Then
                            Dim length = uiWindow.elementDetailData(index).elementHeight
                            BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)
                            Dim MemberWeightPerFT = BoxTrackWeight + BoxStudWeight
                            ''the end post weight= (2.07+1.73)*199.75/12=126.5 pound
                            Dim NumberOfCut = 8
                            NumberOfScrew = ((length / 12) / data.jambScrewSpa) * 4
                            PrintCost(index, MemberWeightPerFT, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                        Else
                            GoTo studCalculate
                        End If
                    Case "H.Blocking"
                        'If BoxWidth > 2.5 Then
                        Dim MemeberWeightPerfeet = ((BoxWidth + 2 * studFleng + 0.75 * 2) * steelThickness / 144) * 490
                        Dim NumberOfCut = 2
                        NumberOfScrew = 4
                        PrintCost(index, MemeberWeightPerfeet, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                        'End If
                    Case "V.Blocking"
                        Dim MemberWeightPerFT = ((BoxWidth + 2 * studFleng + 0.75 * 2) * steelThickness / 144) * 490
                        Dim NumberOfCut = 2
                        NumberOfScrew = 4
                        PrintCost(index, MemberWeightPerFT, NumberOfScrew, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case "Bottom Track"
                        'Dim BoxTrackWeight = (BoxWidth + 2 * data.trackFlange) * (steelThickness / 144) * 490
                        Dim MemberWeightPerFT = BoxTrackWeight
                        Dim NumberOfCut = 2
                        PrintCost(index, MemberWeightPerFT, 0, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case "Opening Bottom Track"
                        Dim BoxTrackWeight = (BoxWidth + 2 * data.trackFlange) * (steelThickness / 144) * 490
                        Dim MemberWeightPerFT = BoxTrackWeight
                        Dim NumberOfCut = 2
                        PrintCost(index, MemberWeightPerFT, 0, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case "Opening Top Track"
                        Dim BoxTrackWeight = (BoxWidth + 2 * data.trackFlange) * (steelThickness / 144) * 490
                        Dim MemberWeightPerFT = BoxTrackWeight
                        Dim NumberOfCut = 2
                        PrintCost(index, MemberWeightPerFT, 0, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case "Top Track"
                        'StudWeight = ((data.studsDepth + 2 * data.trackFlange + 0.75 * 2) * steelThickness / 144) * 490
                        Dim MemberWeightPerFT = BoxTrackWeight
                        Dim NumberOfCut = 2
                        PrintCost(index, MemberWeightPerFT, 0, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case "Header"
                        Dim length = uiWindow.elementDetailData(index).elementHeight
                        BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)
                        Dim MemberWeightPerFT = BoxWeight
                        Dim NumberOfCut = 8
                        Dim screw = ((length / BoxDepth) * 4)
                        PrintCost(index, MemberWeightPerFT, screw, Me.uiWindow.elementDetailData(index).elementName, NumberOfCut)
                    Case Else
                        ''''''BoxWeightCalcualtion(BoxWidth, steelThickness, trackFelnge, BoxDepth, StudsWidth)

                        Dim numOfScr As Double = Math.Round((((uiWindow.elementDetailData(index).elementHeight) / 12) / data.jambScrewSpa) * 4)
                        PrintCost(index, BoxWeight, numOfScr, Me.uiWindow.elementDetailData(index).elementName, 2)
                End Select
            Else
studCalculate:
                Dim type = uiWindow.elementDetailData(index).elementName
                Select Case type
                    Case "Shear Plate Left"
                        While posShPlate < 1999 And data.ShearScrewOpening(posShPlate) = 0
                            posShPlate += 1
                        End While
                        NumberOfScrew = data.ShearScrewOpening(posShPlate) * 4
                        posShPlate += 1
                        PrintCost(index, studWeightPerft, NumberOfScrew, "stud", 2)
                    Case "Shear Plate Right"
                        NumberOfScrew = data.ShearScrewOpening(posShPlate) * 4
                        PrintCost(index, studWeightPerft, NumberOfScrew, "stud", 2)
                    Case Else
                        NumberOfScrew = 4
                        PrintCost(index, studWeightPerft, NumberOfScrew, "stud", 2)
                End Select
            End If

        Next
        Me.uiWindow.panelWeightLb.Text = Double.Parse(totalWeight).ToString("0.00")
        Me.uiWindow.totalCost.Text = Double.Parse(totalCost).ToString("0.00")
        Me.uiWindow.LabourCost.Text = Double.Parse(totalLabourCost).ToString("0.00")
        Me.uiWindow.labourTimeHr.Text = Double.Parse(totalLabourHr).ToString("0.00") '.ToString("0.00")
        Me.uiWindow.screws.Text = totalScrew
        Me.uiWindow.materialCost.Text = Double.Parse(totalMaterialCost).ToString("0.00")


        Dim bold = New Setter(TextBlock.FontWeightProperty, FontWeights.Bold)

        Me.uiWindow.costCalcData.Add(New CostCalculationsData() With {
        .element = " ",
        .member = "Total",
        .totalCost = Double.Parse(totalCost).ToString("0.00"),
        .labourTotalHr = Double.Parse(totalLabourHr).ToString("0.00"),
        .screw = Double.Parse(totalScrew).ToString("0.00"),
        .totalWeight = Double.Parse(totalWeight).ToString("0.00"),
        .labourCost = Double.Parse(totalLabourCost).ToString("0.00"),
        .totalMaterialCost = Double.Parse(totalMaterialCost).ToString("0.00"),
        .cutsNumber = Double.Parse(totalNumberOfCuts).ToString("0.00"),
        .CutTimeHr = Double.Parse(totalTimeCutHour).ToString("0.00"),
        .installTimeHr = Double.Parse(totalInstallationTime).ToString("0.00"),
        .CutCost = Double.Parse(totalCutCost).ToString("0.00"),
        .costLabScre = Double.Parse(totalScrewLabourCost).ToString("0.00"),
        .materialScrewCost = Double.Parse(totalScrewMaterialCost).ToString("0.00"),
        .costMember = Double.Parse(totalCostMemeber).ToString("0.00"),
        .timeToScrewHr = Double.Parse(totalTimeOfScrew).ToString("0.00"),
        .installCost = Double.Parse(totalInstallationCost).ToString("0.00"),
        .lengthInch = Double.Parse(totallength).ToString("0.00")
        })

        Return 'halt procedure
calculateSudWeight:
        StudWeight = ((studDepth + 2 * studFleng + 0.75 * 2) * steelThickness / 144) * 490
        '      StudsWeight = ((StudsDepthneeds + 2 * StudsWidth + 0.75 * 2) * SteelThickness / 144) * 490 
        MemeberWeightPerFT = StudWeight

        studWeightPerft = StudWeight
        GoTo calculateCost
    End Sub
    Public Sub ShopDrawingForm_Paint(e As Graphics, inputData As InputData, Height As Integer, Width As Integer)
        MainForm = New Main()
        Me.Height = Height
        Me.width = Width
        MainForm.setInputData(inputData)
        n = 0
        If CheckData() Then
            'Kamel code
            verticalElement_temp = 0
            VerticalElement = 0
            KamelStudCode(e)
            Return
        End If
    End Sub
    Private Sub LeftShearPlateWeightCalculation()

    End Sub
    Private Sub RightShearPlateWeightCalculation()

    End Sub

    Private Sub SpliteHorizontalStrapInTwo(k As Integer, j As Integer, NumberOfHorizontalStrap As Integer, m As Integer)

        Dim data = MainForm.inputData
        data.HorizontalStrapHeightBx(k) = data.HorizontalStrapHeightAx(k)
        data.HorizontalStrapHeightBy(k) = data.HorizontalStrapHeightBy(k)
        Dim Rightlimtstrap = data.HorizontalStrapHeightCx(k)
        data.HorizontalStrapHeightCx(k) = data.LeftXBoundary(j)
        data.HorizontalStrapHeightCy(k) = data.HorizontalStrapHeightCy(k)
        data.HorizontalStrapHeightDx(k) = data.HorizontalStrapHeightCx(k)
        data.HorizontalStrapHeightDy(k) = data.HorizontalStrapHeightDy(k)
        data.HorizontalStrapHeightAy(k) = data.HorizontalStrapHeightCy(k)
        data.HorizontalStrapHeightAx(NumberOfHorizontalStrap + m) = data.RightXboundary(j)
        data.HorizontalStrapHeightAy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeight(k)
        data.HorizontalStrapHeightAy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightAy(NumberOfHorizontalStrap + m)
        'b
        data.HorizontalStrapHeightBx(NumberOfHorizontalStrap + m) = data.RightXboundary(j)
        data.HorizontalStrapHeightBy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightBy(k)
        'C
        data.HorizontalStrapHeightCx(NumberOfHorizontalStrap + m) = Rightlimtstrap
        data.HorizontalStrapHeightCy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightCy(k)
        'D
        data.HorizontalStrapHeightDx(NumberOfHorizontalStrap + m) = Rightlimtstrap
        data.HorizontalStrapHeightDy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightDy(k)
        data.HorizontalStrapHeightLength(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightCx(NumberOfHorizontalStrap + m) - data.HorizontalStrapHeightAx(NumberOfHorizontalStrap + m)
        data.HorizontalStrapHeightAy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightCy(k)
        Return
    End Sub

    Private Sub SpliteHorizontalStrapInTwoHeight(k As Integer, j As Integer)
        Dim data = MainForm.inputData

        If data.HorizontalStrapHeightAy(k) + data.SizeHorizontalStrapHeight(k) >= data.SillOpening(j) And data.HorizontalStrapHeightAy(k) <= data.HeightOpening(j) + data.SillOpening(j) Then
            m = m + 1
            SpliteHorizontalStrapInTwo(k, j, data.TotalNumberOfHorizontalStrips, m)
        End If
        Return
    End Sub

    Dim m = 0
    Dim numberofStrips = 0
    Dim totalNumberOfStuds = 0

    Private Sub DrawHorizontalStrips(RatioX As Double, RatioY As Double, OffsetX_X As Double, OffsetY_Y As Double, g As Graphics)
        ''Draw the   horizontal strips 
        Dim data = MainForm.inputData

        For i = 0 To 199
            data.SizeHorizontalStrapHeight(i) = 0
            data.HorizontalStrapHeightAx(i) = 0
            data.HorizontalStrapHeightAy(i) = 0

            data.HorizontalStrapHeightBx(i) = 0
            data.HorizontalStrapHeightBy(i) = 0

            data.HorizontalStrapHeightCx(i) = 0
            data.HorizontalStrapHeightCy(i) = 0
            data.HorizontalStrapHeightDx(i) = 0
            data.HorizontalStrapHeightDy(i) = 0
        Next

        Dim NumberOfHorizontalStrap = data.TotalNumberOfHorizontalStrips
        Dim LeftXb(100) As Double, RightXb(100) As Double, SillOpening(100) As Double, HeightOpening(100), centerOpening(100), widthOpening(100)
        For j = 0 To data.numberOfOpenings - 1
            LeftXb(j) = data.LeftXBoundary(j)
            RightXb(j) = data.RightXboundary(j)
            SillOpening(j) = data.SillOpening(j)
            HeightOpening(j) = data.HeightOpening(j)
            centerOpening(j) = data.CenterOpening(j)
            widthOpening(j) = data.WidthOpening(j)
        Next

        For j = 0 To data.numberOfOpenings - 1
            For k = j + 1 To data.numberOfOpenings - 1
                If LeftXb(j) > LeftXb(k) Then
                    LeftXb(j) = LeftXb(j) Xor LeftXb(k)
                    LeftXb(k) = LeftXb(k) Xor LeftXb(j)
                    LeftXb(j) = LeftXb(j) Xor LeftXb(k)

                    HeightOpening(j) = HeightOpening(j) Xor HeightOpening(k)
                    HeightOpening(k) = HeightOpening(k) Xor HeightOpening(j)
                    HeightOpening(j) = HeightOpening(j) Xor HeightOpening(k)

                    SillOpening(j) = SillOpening(j) Xor SillOpening(k)
                    SillOpening(k) = SillOpening(k) Xor SillOpening(j)
                    SillOpening(j) = SillOpening(j) Xor SillOpening(k)

                    centerOpening(j) = centerOpening(j) Xor centerOpening(k)
                    centerOpening(k) = centerOpening(k) Xor centerOpening(j)
                    centerOpening(j) = centerOpening(j) Xor centerOpening(k)

                    widthOpening(j) = widthOpening(j) Xor widthOpening(k)
                    widthOpening(k) = widthOpening(k) Xor widthOpening(j)
                    widthOpening(j) = widthOpening(j) Xor widthOpening(k)

                    RightXb(j) = RightXb(j) Xor RightXb(k)
                    RightXb(k) = RightXb(k) Xor RightXb(j)
                    RightXb(j) = RightXb(j) Xor RightXb(k)
                End If
            Next k
        Next j

        m = 0
        For i = 1 To NumberOfHorizontalStrap
            data.SizeHorizontalStrapHeight(i) = data.HorizontalStripDepth(i)
            data.HorizontalStrapHeightAx(i) = 0
            data.HorizontalStrapHeightAy(i) = data.HorizontalStrapHeight(i)
            data.HorizontalStrapHeightBx(i) = 0
            data.HorizontalStrapHeightBy(i) = data.HorizontalStrapHeight(i) + data.SizeHorizontalStrapHeight(i)
            data.HorizontalStrapHeightCx(i) = data.wallLength
            data.HorizontalStrapHeightCy(i) = data.HorizontalStrapHeight(i)
            data.HorizontalStrapHeightDx(i) = data.wallLength
            data.HorizontalStrapHeightDy(i) = data.HorizontalStrapHeight(i) + data.SizeHorizontalStrapHeight(i)
            ' GoSub HorizontalStrapweightCalculation
            data.HorizontalStrapHeightLength(i) = data.HorizontalStrapHeightCx(i) - data.HorizontalStrapHeightAx(i)
        Next i

        For i = 1 To NumberOfHorizontalStrap
            Dim pos = 0
            While pos < verticalElement_temp
                If (data.HorizontalStrapHeightBy(i) > data.BYY(pos)) = False Then
                    data.HorizontalStrapHeightAx(i) = data.BXX(pos)
                    data.HorizontalStrapHeightBx(i) = data.BXX(pos)

                    Dim rev = verticalElement_temp

                    While rev > 0
                        If (data.HorizontalStrapHeightDy(i) > data.DYY(rev)) = False Then
                            data.HorizontalStrapHeightCx(i) = data.CXX(rev)
                            data.HorizontalStrapHeightDx(i) = data.CXX(rev)
                            GoTo interate
                        End If

                        rev -= 1
                    End While

                    GoTo interate
                End If
                pos += 1
            End While
interate:
        Next
        'remove intersection with opening
        For j = 0 To data.numberOfOpenings - 1
            For k = 1 To NumberOfHorizontalStrap + m
                If (data.HorizontalStrapHeightAy(k) >= SillOpening(j) And data.HorizontalStrapHeightAy(k) <= (SillOpening(j) + HeightOpening(j)) Or data.HorizontalStrapHeightBy(k) <= ((SillOpening(j) + HeightOpening(j))) And (data.HorizontalStrapHeightBy(k) >= SillOpening(j))) And data.HorizontalStrapHeightCx(k) >= LeftXb(j) Then
                    'close first element
                    data.HorizontalStrapHeightCx(k) = LeftXb(j)
                    data.HorizontalStrapHeightDx(k) = LeftXb(j)
                    data.HorizontalStrapHeightLength(k) = data.HorizontalStrapHeightCx(k) - data.HorizontalStrapHeightAx(k)
                    'create new element starting from right end of opening
                    m += 1
                    data.HorizontalStrapHeightAx(NumberOfHorizontalStrap + m) = RightXb(j)
                    data.HorizontalStrapHeightBx(NumberOfHorizontalStrap + m) = RightXb(j)
                    data.HorizontalStrapHeightAy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightAy(k)
                    data.HorizontalStrapHeightBy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightBy(k)
                    data.HorizontalStrapHeightCx(NumberOfHorizontalStrap + m) = data.wallLength
                    data.HorizontalStrapHeightDx(NumberOfHorizontalStrap + m) = data.wallLength
                    data.HorizontalStrapHeightCy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightCy(k)
                    data.HorizontalStrapHeightDy(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightDy(k)
                    data.HorizontalStrapHeightLength(NumberOfHorizontalStrap + m) = data.HorizontalStrapHeightCx(NumberOfHorizontalStrap + m) - data.HorizontalStrapHeightAx(NumberOfHorizontalStrap + m)
                End If
            Next k
        Next j
        'sort 
outside:
        For index As Integer = 1 To NumberOfHorizontalStrap + m
            Dim a As Point = New Point(data.HorizontalStrapHeightAx(index) * RatioX + OffsetX_X, OffsetY_Y - data.HorizontalStrapHeightAy(index) * RatioY)
            Dim b As Point = New Point(data.HorizontalStrapHeightBx(index) * RatioX + OffsetX_X, OffsetY_Y - data.HorizontalStrapHeightBy(index) * RatioY)
            Dim c As Point = New Point(data.HorizontalStrapHeightCx(index) * RatioX + OffsetX_X, OffsetY_Y - data.HorizontalStrapHeightCy(index) * RatioY)
            Dim d As Point = New Point(data.HorizontalStrapHeightDx(index) * RatioX + OffsetX_X, OffsetY_Y - data.HorizontalStrapHeightDy(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.DarkOrange, b, d) 'Line BD
            g.DrawLine(Brushes.DarkOrange, a, c) 'Line AC
            g.DrawLine(Brushes.DarkOrange, a, b) 'Line AB
            g.DrawLine(Brushes.DarkOrange, c, d) 'Line CD
        Next
        numberofStrips = NumberOfHorizontalStrap + m
    End Sub
    Private Sub CalculateSharePlate()
        'shear plate right shear plate rightshear plate rightshear plate rightshear plate rightshear plate rightshear plate right
        Dim data = MainForm.inputData
        TotalNumberOfmember = TotalNumberOfmember + data.NumberOfOpening

        For i = 0 To 19
            data.ShearPlateLeft(i) = 0
            data.ShearPlateLeftAX(i) = 0
            data.ShearPlateLeftAY(i) = 0
            data.ShearPlateLeftBX(i) = 0
            data.ShearPlateLeftBY(i) = 0
            data.ShearPlateLeftCX(i) = 0
            data.ShearPlateLeftCY(i) = 0
            data.ShearPlateLeftDX(i) = 0
            data.ShearPlateLeftDY(i) = 0

            data.ShearPlateRightAX(i) = 0
            data.ShearPlateRightAY(i) = 0
            data.ShearPlateRightBX(i) = 0
            data.ShearPlateRightBY(i) = 0
            data.ShearPlateRightCX(i) = 0
            data.ShearPlateRightCY(i) = 0
            data.ShearPlateRightDX(i) = 0
            data.ShearPlateRightDY(i) = 0
            data.ShearPlateRight(i) = 0
        Next
        'Cells(3, 10) = TotalNumberOfmember
        For i = 0 To data.NumberOfOpening
            If data.ShearPLate$(i) = "True" Then
                Dim JambWidth As Double = data.JambWidthLeft(i) 'Cells(16 + i, 9) 'jamb width left
                data.ShearPlateLeftAX(i) = data.LeftXBoundary(i) + (1 / 2) - JambWidth
                data.ShearPlateLeftAY(i) = data.SillOpening(i) + data.HeightOpening(i) + (1 / 2)
                data.ShearPlateLeftBX(i) = data.LeftXBoundary(i) + (1 / 2) - JambWidth
                data.ShearPlateLeftBY(i) = data.HeightOpening(i) + data.SillOpening(i) + data.Header(i) - (1 / 2)
                data.ShearPlateLeftCX(i) = data.ShearPlateLeftAX(i) + JambWidth * 2
                data.ShearPlateLeftCY(i) = data.SillOpening(i) + data.HeightOpening(i) + (1 / 2)
                data.ShearPlateLeftDX(i) = data.ShearPlateLeftAX(i) + JambWidth * 2
                data.ShearPlateLeftDY(i) = data.HeightOpening(i) + data.SillOpening(i) + data.Header(i) - (1 / 2)
                data.ShearPlateLeft(i) = JambWidth * 2 - (1 / 2)
                Dim ShearPLateLegnth = data.ShearPlateLeft(i)
                Dim ShearPLateHeight = data.ShearPlateLeftAY(i) - data.ShearPlateLeftBY(i)
                LeftShearPlateWeightCalculation()
            Else
                data.ShearPlateLeft(i) = 0
                data.ShearPlateLeftAX(i) = 0
                data.ShearPlateLeftAY(i) = 0
                data.ShearPlateLeftBX(i) = 0
                data.ShearPlateLeftBY(i) = 0
                data.ShearPlateLeftCX(i) = 0
                data.ShearPlateLeftCY(i) = 0
                data.ShearPlateLeftDX(i) = 0
                data.ShearPlateLeftDY(i) = 0
            End If

        Next i

        For i = 0 To data.NumberOfOpening
            If data.ShearPLate$(i) = "True" Then
                Dim JambWidth = data.JambWidthRight(i) 'Cells(16 + i, 10) 'jamb width right
                data.ShearPlateRightAX(i) = data.RightXboundary(i) - (1 / 2) + JambWidth
                data.ShearPlateRightAY(i) = data.SillOpening(i) + data.HeightOpening(i) + 1 / 2
                data.ShearPlateRightBX(i) = data.RightXboundary(i) - 1 / 2 + JambWidth
                data.ShearPlateRightBY(i) = data.HeightOpening(i) + data.SillOpening(i) + data.Header(i) - 1 / 2
                data.ShearPlateRightCX(i) = data.ShearPlateRightAX(i) - JambWidth * 2
                data.ShearPlateRightCY(i) = data.SillOpening(i) + data.HeightOpening(i) + 1 / 2
                data.ShearPlateRightDX(i) = data.ShearPlateRightAX(i) - JambWidth * 2
                data.ShearPlateRightDY(i) = data.HeightOpening(i) + data.SillOpening(i) + data.Header(i) - 1 / 2
                data.ShearPlateRight(i) = JambWidth * 2 - 1 / 2

                Dim ShearPLateLegnth = data.ShearPlateRight(i)
                Dim ShearPLateHeight = data.ShearPlateRightAY(i) - data.ShearPlateRightBY(i)
                'GoSub RightShearPlateWeightCalculation
                RightShearPlateWeightCalculation()
            Else
                data.ShearPlateRightAX(i) = 0
                data.ShearPlateRightAY(i) = 0
                data.ShearPlateRightBX(i) = 0
                data.ShearPlateRightBY(i) = 0
                data.ShearPlateRightCX(i) = 0
                data.ShearPlateRightCY(i) = 0
                data.ShearPlateRightDX(i) = 0
                data.ShearPlateRightDY(i) = 0
                data.ShearPlateRight(i) = 0

            End If
        Next i
    End Sub
    Private Sub RemoveInterectionStudWithInterOrAdditionalPost()
        '''''''''''''''Studs positioned inside jamb case'''''''''''''''''''''
        Dim data = MainForm.inputData
        Dim TotalNumberOfInterPost = data.NumberOfInteriorPost
        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointAX(i) > data.InterMetdiatPostAX(inter) And data.InterMetdiatPostCX(inter) > data.StudsPointCX(i) And (data.InterMetdiatPostCY(inter) = data.StudsPointCY(i)) Then
                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                    removedStuds += 1
                End If
            Next
        Next inter
        ''''''''''''''''''''''''''''''''''''

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointCX(i) >= data.InterMetdiatPostAX(inter) And data.InterMetdiatPostAX(inter) >= data.StudsPointAX(i) And (data.InterMetdiatPostCY(inter) = data.StudsPointCY(i)) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next inter

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointAX(i) <= data.InterMetdiatPostAX(inter) And data.InterMetdiatPostAX(inter) <= data.StudsPointCX(i) And data.InterMetdiatPostBY(inter) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.InterMetdiatPostCX(inter) And data.InterMetdiatPostCX(inter) <= data.StudsPointCX(i) And (data.InterMetdiatPostCY(inter) = data.StudsPointCY(i))) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next inter

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointAX(i) <= data.InterMetdiatPostCX(inter) And data.InterMetdiatPostCX(inter) <= data.StudsPointCX(i) And data.InterMetdiatPostBY(inter) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.InterMetdiatPostCX(inter) And data.InterMetdiatPostCX(inter) <= data.StudsPointCX(i) And (data.InterMetdiatPostCY(inter) = data.StudsPointCY(i))) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next inter

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If (data.StudsPointAX(i) = data.InterMetdiatPostCX(inter)) And data.InterMetdiatPostCY(inter) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.InterMetdiatPostDY(inter) Then 'And data.JambAX(jamb) <= data.StudsPointCX(i) And  Then 'And Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If 'And (data.JambBY(jamb) = data.StudsPointBY(i)) 
            Next i
        Next inter

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If (data.StudsPointCX(i) = data.InterMetdiatPostAX(inter)) And data.InterMetdiatPostCY(inter) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.InterMetdiatPostDY(inter) Then 'And data.JambAX(jamb) <= data.StudsPointCX(i) And  Then 'And Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If 'And (data.JambBY(jamb) = data.StudsPointBY(i)) 
            Next i
        Next inter

        For inter = 1 To TotalNumberOfInterPost
            For i = 0 To VerticalElement + n - 1

                If (data.StudsPointCX(i) >= data.InterMetdiatPostAX(inter)) And (data.StudsPointCX(i) - data.InterMetdiatPostAX(inter)) < (data.StudsWidth) And data.InterMetdiatPostCY(inter) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.InterMetdiatPostDY(inter) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If 'And (data.JambBY(jamb) = data.StudsPointBY(i)) 
            Next i
        Next inter
    End Sub

    Public NumbreOfStuds = 0
    Public scalePortion = 20
    Public scaleDrawing = 0.6
    Public Function ConvertScaleToPrintPage(verticalPixelYaxis, totalVerticalInch) As Double
        Dim printSizeVericalLeagerInch = 11
        'convert pixel to inch
        Dim ppi = 96
        Dim totalInchInPixel = verticalPixelYaxis / ppi
        Return totalVerticalInch / totalInchInPixel
    End Function
    Public Function ScaleToDimenions(scaleInch As Double) As Double
        Dim ppi = 96
        Dim verticalPixelYaxis = 1
        Dim totalInchInPixel = MainForm.inputData.HeightYYMaximum / scaleInch
        verticalPixelYaxis = totalInchInPixel * ppi
        Return verticalPixelYaxis
    End Function



    Private Xfactor As Double
    Private XOffset As Double
    Private YOffset As Double
    Private graphic As Graphics
    Private Sub KamelStudCode(e As Graphics)
        Dim data = MainForm.inputData
        data.ClearCut = MainForm.inputData.studClerance

        For i = 0 To 200 - 1
            data.StudsPointAX(i) = 0
            data.StudsPointAY(i) = 0

            data.StudsPointBY(i) = 0
            data.StudsPointBX(i) = 0

            data.StudsPointCX(i) = 0
            data.StudsPointCY(i) = 0

            data.StudsPointDX(i) = 0
            data.StudsPointDY(i) = 0
        Next
        'If scaleDrawing <= 0.6 Then
        '    Me.uiWindow.CanvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled
        'Else
        '    Me.uiWindow.CanvasScroll.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto
        'End If

        Dim g = e
        graphic = e
        VerticalElement = 0
        n = 0
        TotalNumberOfElement = 0
        removedStuds = 0

        Dim XRatio = (width - ((scalePortion * width) / 100)) / data.WallLengthXMaximum
        Dim HeightYYMaximum1 = data.HeightYYMaximum
        Dim RatioY = XRatio * scaleDrawing '(Height - ((20 * Height) / 100)) / HeightYYMaximum1
        Dim RatioX = XRatio * scaleDrawing

        Dim OffsetX_X = 50

        If BaseWidth < 1 And BaseHeight < 1 Then
            BaseHeight = uiWindow.CanvasArea.ActualHeight
            BaseWidth = uiWindow.CanvasArea.ActualWidth
            BaseOffsetY = BaseHeight

        End If

        Dim ScaleRquired = 0
        Dim requiredScale As Double = 0
        If Double.TryParse(uiWindow.adjustFactorTxtBox.Text, ScaleRquired) Then
            requiredScale = ScaleRquired
        End If

        If requiredScale > 0 Then
            Dim pixelsForHeight As Double = ScaleToDimenions(requiredScale)
            Dim requiredRatio As Double = pixelsForHeight / HeightYYMaximum1
            Dim scalingRequired = requiredRatio / XRatio
            scaleDrawing = scalingRequired
            RatioX = XRatio * scaleDrawing

            RatioY = RatioX
            uiWindow.adjustFactorTxt.Value = scaleDrawing

        End If

        If (scaleDrawing * 100) < 65 And (requiredScale > 0) = False Then
            scaleDrawing = 0.0
        End If

        Me.uiWindow.scaleTxtLbl.Content = ConvertScaleToPrintPage(HeightYYMaximum1 * RatioX, HeightYYMaximum1).ToString("0.00") + " Inch"
        '                                                       height                  -                margin
        Dim OffsetY_Y = Height - 100


        Dim number As Decimal = data.wallLength / data.studsSpacing
        NumbreOfStuds = Math.Round(number)
        Dim Checklength = data.studsSpacing * (NumbreOfStuds)

        If Checklength = data.wallLength Then NumbreOfStuds = NumbreOfStuds - 1

        For st = 0 To NumbreOfStuds
            data.StudsPointAX(st) = (st) * data.studsSpacing
            Dim BB = data.StudsPointAX(st)

            If data.StudsPointAX(st) > data.wallLength Then
                data.StudsPointAX(st) = data.wallLength
            End If
        Next st

        Dim VerticalElementWidth As Double
        Dim xx As Double
        For st = 0 To NumbreOfStuds
            data.StudsPointAX(st) = data.StudsPointAX(st)
            data.StudsPointAY(st) = data.ClearCut
            VerticalElementWidth = data.StudsWidth
            xx = data.StudsPointAX(st)
            KamelElementHeightCalculation(xx, VerticalElementWidth)
            data.StudsPointBX(st) = data.StudsPointAX(st)
            data.StudsPointBY(st) = WallHeightX - data.ClearCut
            data.StudsLength(st) = data.StudsPointBY(st) - data.StudsPointAY(st)
            data.StudsPointCX(st) = data.StudsPointAX(st) + data.StudsWidth
            data.StudsPointCY(st) = data.ClearCut
            data.StudsPointDX(st) = data.StudsPointCX(st)
            data.StudsPointDY(st) = WallHeightX - data.ClearCut
            data.Item$(st) = "Studs"
            data.VERTICALMEMBER(st) = "Studs"
            VerticalElement = VerticalElement + 1
        Next st

        Dim numberOfStuds = NumbreOfStuds

        If Math.Round(data.wallLength - data.StudsPointAX(NumbreOfStuds)) > data.studsSpacing Then
            numberOfStuds = numberOfStuds + 1
            NumbreOfStuds += 1
        End If

        If (data.wallLength - data.StudsPointCX(numberOfStuds)) > data.studsFlange Then
            numberOfStuds += 1
            NumbreOfStuds += 1
        End If

        data.StudsPointAX(numberOfStuds) = data.wallLength 'data.StudsPointAX(NumbreOfStuds)
        data.StudsPointAY(numberOfStuds) = data.ClearCut
        VerticalElementWidth = data.StudsWidth

        xx = data.StudsPointAX(numberOfStuds) - data.StudsWidth

        KamelElementHeightCalculation(xx, VerticalElementWidth)

        data.StudsPointBX(numberOfStuds) = data.StudsPointAX(numberOfStuds)
        data.StudsPointBY(numberOfStuds) = WallHeightX - data.ClearCut

        data.StudsLength(numberOfStuds) = data.StudsPointBY(numberOfStuds) - data.StudsPointAY(numberOfStuds)
        data.StudsPointCX(numberOfStuds) = data.StudsPointBX(numberOfStuds) - data.StudsWidth

        data.StudsPointCY(numberOfStuds) = data.ClearCut

        data.StudsPointDX(numberOfStuds) = data.StudsPointCX(numberOfStuds)
        data.StudsPointDY(numberOfStuds) = WallHeightX - data.ClearCut
        data.StudsLength(numberOfStuds) = data.StudsPointBY(numberOfStuds) - data.StudsPointAY(numberOfStuds)
        data.VERTICALMEMBER(numberOfStuds) = "Studs"


        '>>>>---------> End of studs creation
        data.NumTotalNumberoFstuds = NumbreOfStuds
        'Calculate studs for opening
        openingStuds(NumbreOfStuds, VerticalElement)
        ' Calculate for Jamb Drawing.
        KamelCodeDrawJamb(VerticalElement + n)
        RemoveIntractionJambWithStud()
        ''REMAING
        CreateStartEndPost()
        RemoveIntractionEndPostStud()
        ''Calculate interior posts 
        KamelCodeInterPost()
        RemoveInterectionStudWithInterOrAdditionalPost()
        DrawTopBottomTrack()
        CalculateSharePlate()

        calculateIntersectionWall()
        IntersectionWallIntesectionStudRemovel()

        For index As Integer = 0 To data.NumberofIntersectionWall - 1
            Dim a As Point = New Point(data.IntersectionWallAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.IntersectionWallAY(index) * RatioY)
            Dim b As Point = New Point(data.IntersectionWallBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.IntersectionWallBy(index) * RatioY)
            Dim c As Point = New Point(data.IntersectionWallcX(index) * RatioX + OffsetX_X, OffsetY_Y - data.IntersectionWallcy(index) * RatioY)
            Dim d As Point = New Point(data.IntersectionWalldX(index) * RatioX + OffsetX_X, OffsetY_Y - data.IntersectionWalldy(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.Black, b, d) 'Line BD
            g.DrawLine(Brushes.Black, a, c) 'Line AC
            g.DrawLine(Brushes.Black, a, b) 'Line AB
            g.DrawLine(Brushes.Black, c, d) 'Line CD

        Next index


        For index As Integer = 0 To VerticalElement + n
            Dim a As Point = New Point(data.StudsPointAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.StudsPointAY(index) * RatioY)
            Dim b As Point = New Point(data.StudsPointBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.StudsPointBY(index) * RatioY)
            Dim c As Point = New Point(data.StudsPointCX(index) * RatioX + OffsetX_X, OffsetY_Y - data.StudsPointCY(index) * RatioY)
            Dim d As Point = New Point(data.StudsPointDX(index) * RatioX + OffsetX_X, OffsetY_Y - data.StudsPointDY(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.Blue, b, d) 'Line BD
            g.DrawLine(Brushes.Blue, a, c) 'Line AC
            g.DrawLine(Brushes.Blue, a, b) 'Line AB
            g.DrawLine(Brushes.Blue, c, d) 'Line CD

        Next index
        totalNumberOfStuds = VerticalElement + n - 1

        '' Draw Jamb Left
        For index As Integer = 0 To data.NumberOfOpening - 1

            If data.LeftXBoundary(index) > 0 Then
                'Dim a As Point = New Point(data.StudsPointAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.StudsPointAY(index) * RatioY)
                Dim a As Point = New Point(data.JambLeftXA(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambLeftYA(index) * RatioY)
                Dim b As Point = New Point(data.JambLeftXB(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambLeftYB(index) * RatioY)
                Dim c As Point = New Point(data.JambLeftXC(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambLeftYC(index) * RatioY)
                Dim d As Point = New Point(data.JambLeftXD(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambLeftYD(index) * RatioY)
                'B----D
                '|    |
                'A -- C
                g.DrawLine(Brushes.DeepPink, b, d) 'Line BD
                g.DrawLine(Brushes.DeepPink, a, c) 'Line AC
                g.DrawLine(Brushes.DeepPink, a, b) 'Line AB
                g.DrawLine(Brushes.DeepPink, c, d) 'Line CD
            End If
        Next index

        ' Draw Jamb Right
        'Issue 
        For index As Integer = 0 To data.NumberOfOpening - 1
            If data.RightXboundary(index) > 0 Then


                Dim a As Point = New Point(data.JambRightXA(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambRightYA(index) * RatioY)
                Dim b As Point = New Point(data.JambRightXB(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambRightYB(index) * RatioY)
                Dim c As Point = New Point(data.JambRightXC(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambRightYC(index) * RatioY)
                Dim d As Point = New Point(data.JambRightXD(index) * RatioX + OffsetX_X, OffsetY_Y - data.JambRightYD(index) * RatioY)
                'B----D
                '|    |
                'A -- C
                g.DrawLine(Brushes.DeepPink, b, d) 'Line BD
                g.DrawLine(Brushes.DeepPink, a, c) 'Line AC
                g.DrawLine(Brushes.DeepPink, a, b) 'Line AB
                g.DrawLine(Brushes.DeepPink, c, d) 'Line CD
            End If
        Next index

        If data.endPostBox Then

            For index As Integer = 1 To 2 * data.endOfPost
                Dim a As Point = New Point(data.EndPostStXA(index) * RatioX + OffsetX_X, OffsetY_Y - data.EndPostStYA(index) * RatioY)
                Dim b As Point = New Point(data.EndPostStXB(index) * RatioX + OffsetX_X, OffsetY_Y - data.EndPostStYB(index) * RatioY)
                Dim c As Point = New Point(data.EndPostStXC(index) * RatioX + OffsetX_X, OffsetY_Y - data.EndPostStYC(index) * RatioY)
                Dim d As Point = New Point(data.EndPostStXD(index) * RatioX + OffsetX_X, OffsetY_Y - data.EndPostStYD(index) * RatioY)
                'B----D
                '|    |
                'A -- C
                g.DrawLine(Brushes.DarkGoldenrod, b, d) 'Line BD
                g.DrawLine(Brushes.DarkGoldenrod, a, c) 'Line AC
                g.DrawLine(Brushes.DarkGoldenrod, a, b) 'Line AB
                g.DrawLine(Brushes.DarkGoldenrod, c, d) 'Line CD
            Next

        End If

        'Draw interpost
        For index As Integer = 1 To data.NumberOfInteriorPost
            Dim a As Point = New Point(data.InterMetdiatPostAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.InterMetdiatPostAY(index) * RatioY)
            Dim b As Point = New Point(data.InterMetdiatPostBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.InterMetdiatPostBY(index) * RatioY)
            Dim c As Point = New Point(data.InterMetdiatPostCX(index) * RatioX + OffsetX_X, OffsetY_Y - data.InterMetdiatPostCY(index) * RatioY)
            Dim d As Point = New Point(data.InterMetdiatPostDX(index) * RatioX + OffsetX_X, OffsetY_Y - data.InterMetdiatPostDY(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.Brown, b, d) 'Line BD
            g.DrawLine(Brushes.Brown, a, c) 'Line AC
            g.DrawLine(Brushes.Brown, a, b) 'Line AB
            g.DrawLine(Brushes.Brown, c, d) 'Line CD
        Next

        For index As Integer = 1 To data.NumberOfOpening - 1
            Dim a As Point = New Point(data.TopTrackAXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.TopTrackAYO(index) * RatioY)
            Dim b As Point = New Point(data.TopTrackBXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.TopTrackBYO(index) * RatioY)
            Dim c As Point = New Point(data.TopTrackCXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.TopTrackCYO(index) * RatioY)
            Dim d As Point = New Point(data.TopTrackDXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.TopTrackDYO(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.DarkGreen, b, d) 'Line BD
            g.DrawLine(Brushes.DarkGreen, a, c) 'Line AC
            g.DrawLine(Brushes.DarkGreen, a, b) 'Line AB
            g.DrawLine(Brushes.DarkGreen, c, d) 'Line CD
        Next

        For index As Integer = 1 To data.NumberOfOpening - 1
            Dim a As Point = New Point(data.BottomTrackAXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.BottomTrackAYO(index) * RatioY)
            Dim b As Point = New Point(data.BottomTrackBXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.BottomTrackBYO(index) * RatioY)
            Dim c As Point = New Point(data.BottomTrackCXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.BottomTrackCYO(index) * RatioY)
            Dim d As Point = New Point(data.BottomTrackDXO(index) * RatioX + OffsetX_X, OffsetY_Y - data.BottomTrackDYO(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.DarkGreen, b, d) 'Line BD
            g.DrawLine(Brushes.DarkGreen, a, c) 'Line AC
            g.DrawLine(Brushes.DarkGreen, a, b) 'Line AB
            g.DrawLine(Brushes.DarkGreen, c, d) 'Line CD
        Next

        CalculateHeader()
        For index As Integer = 1 To data.NumberOfOpening - 1
            Dim a As Point = New Point(data.OpeningAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.OpeningAY(index) * RatioY)
            Dim b As Point = New Point(data.OpeningBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.OpeningBY(index) * RatioY)
            Dim c As Point = New Point(data.OpeningCX(index) * RatioX + OffsetX_X, OffsetY_Y - data.OpeningCY(index) * RatioY)
            Dim d As Point = New Point(data.OpeningDX(index) * RatioX + OffsetX_X, OffsetY_Y - data.OpeningDY(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.Aqua, b, d) 'Line BD
            g.DrawLine(Brushes.Aqua, a, c) 'Line AC
            g.DrawLine(Brushes.Aqua, a, b) 'Line AB
            g.DrawLine(Brushes.Aqua, c, d) 'Line CD
        Next
        For index As Integer = 0 To data.NumberOfOpening
            Dim a As Point = New Point(data.ShearPlateLeftAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateLeftAY(index) * RatioY)
            Dim b As Point = New Point(data.ShearPlateLeftBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateLeftBY(index) * RatioY)
            Dim c As Point = New Point(data.ShearPlateLeftCX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateLeftCY(index) * RatioY)
            Dim d As Point = New Point(data.ShearPlateLeftDX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateLeftDY(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.DeepPink, b, d) 'Line BD
            g.DrawLine(Brushes.DeepPink, a, c) 'Line AC
            g.DrawLine(Brushes.DeepPink, a, b) 'Line AB
            g.DrawLine(Brushes.DeepPink, c, d) 'Line CD
        Next

        For index As Integer = 0 To data.NumberOfOpening
            Dim a As Point = New Point(data.ShearPlateRightAX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateRightAY(index) * RatioY)
            Dim b As Point = New Point(data.ShearPlateRightBX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateRightBY(index) * RatioY)
            Dim c As Point = New Point(data.ShearPlateRightCX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateRightCY(index) * RatioY)
            Dim d As Point = New Point(data.ShearPlateRightDX(index) * RatioX + OffsetX_X, OffsetY_Y - data.ShearPlateRightDY(index) * RatioY)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.DarkRed, b, d) 'Line BD
            g.DrawLine(Brushes.DarkRed, a, c) 'Line AC
            g.DrawLine(Brushes.DarkRed, a, b) 'Line AB
            g.DrawLine(Brushes.DarkRed, c, d) 'Line CD
        Next

        DrawTopTrack(g, RatioX, RatioY, OffsetX_X, OffsetY_Y)
        'HorizontalBlocking()
        verticalElement_temp = VerticalElement
        DrawingHorizontalBlocking(RatioX, RatioY, OffsetX_X, OffsetY_Y, g)
        'CreateDXFFile(NumbreOfStuds)
        AssignNumericLableToElements(0, 0, 0, 0, False)
        DrawHorizontalStrips(RatioX, RatioY, OffsetX_X, OffsetY_Y, g)

#Region "Comment"


        'ElementHeightCalculation:

        'For s = 1 To NumberOfSlope

        '    If xx >= StartX(s) And xx < EndX(s) Then
        '        WallSlope = SlopX(s)
        '        If SlopX(s) >= 0 Then VerticalElementWidth = 0
        '        If xx = EndX(s) Then WallSlope = SlopX(s + 1)
        '        WallHeightX = HeightX(s) + (xx - EndX(s - 1) + VerticalElementWidth) * WallSlope
        '    End If

        'Next s

#End Region

        AssignNumericLableToElements(RatioX, RatioY, OffsetX_X, OffsetY_Y, True)


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



        Xfactor = RatioX
        XOffset = OffsetX_X
        YOffset = OffsetY_Y
        DrawDimensionsShopDrawing(data)

        DrawDimensions(data, 50, RatioX, RatioY, OffsetY_Y - data.HeightX(0) * RatioY, OffsetY_Y)


        For index As Integer = 0 To data.NumberOfOpening - 2
            DrawOpeningDimensions(RatioX, RatioY, data.LeftXBoundary(index), data.SillOpening(index), data.HeightOpening(index), data.WidthOpening(index), OffsetY_Y)
            OpeningLowerWidth((index + 1) * 5 * RatioY, RatioX, RatioY, leftBoundary(index), OffsetY_Y)

            If (data.SillOpening(index) = 0) Then
                Continue For
            End If
            OpeningSilHeight(data, index, RatioX, RatioY, data.LeftXBoundary(index), data.SillOpening(index), data.WidthOpening(index), OffsetY_Y)
        Next
        Return
    End Sub


    Private Function MapToPixel(num) As Double
        Return num * Xfactor
    End Function

    Private Sub DrawLine(x1 As Integer, y1 As Integer, x2 As Integer, y2 As Integer)
        Dim pointA = New Point(XOffset + MapToPixel(x1), YOffset - MapToPixel(y1))
        Dim pointB = New Point(XOffset + MapToPixel(x2), YOffset - MapToPixel(y2))

        graphic.DrawLine(Brushes.Black, pointA, pointB)
    End Sub

    Private Sub DrawLabel(lbl As String, x As Integer, y As Integer)

        graphic.DrawLabel(lbl, MapToPixel(3), Brushes.Black, XOffset + MapToPixel(x), YOffset - MapToPixel(y))
    End Sub

    Private Sub DrawDimensionsShopDrawing(data As InputData)

        Dim y As Double
        Dim offset As Double = 0

        ''Draw Dimension for Strap
        offset = -3
        For i = 0 To data.horizantalStrap.Count - 1
            y = data.horizantalStrap(i).locationHeight
            DrawLine(offset, 0, offset, y)

            ''close line from  each end of line. 
            DrawLine(offset - 2, 0, offset + 2, 0)
            DrawLine(offset - 2, y, offset + 2, y)

            DrawLabel(y, offset, y / 2)

            'Make tilt cuts
            DrawLine(offset - 2, -2, offset + 2, 2)
            DrawLine(offset - 2, y - 2, offset + 2, y + 2)


            offset -= 2 * 2
        Next
        ''Draw Dimension for wall intersection 
        offset = -7
        For i = 0 To data.wallIntersection.Count - 1
            Dim x = data.wallIntersection(i).wallLocation
            DrawLine(0, offset, data.wallIntersection(i).wallLocation, offset)

            ''close line from  each end of line. 
            DrawLine(0, offset + 2, 0, offset - 2)

            DrawLine(x, offset + 2, x, offset - 2)

            DrawLabel(x, x / 2, offset + 2)

            'Make tilt cuts
            DrawLine(-2, offset - 2, 2, offset + 2)
            DrawLine(x - 2, offset - 2, x + 2, offset + 2)

            offset -= 7
        Next
        ''Draw Dimension for Additional Post
        For i = 0 To data.additionalPost.Count - 1
            Dim x = data.additionalPost(i).postLocation
            DrawLine(0, offset, x, offset)

            ''close line from  each end of line. 
            DrawLine(0, offset + 2, 0, offset - 2)

            DrawLine(x, offset + 2, x, offset - 2)

            DrawLabel(x, x / 2, offset + 2)

            'Make tilt cuts
            DrawLine(-2, offset - 2, 2, offset + 2)
            DrawLine(x - 2, offset - 2, x + 2, offset + 2)

            offset -= 7
        Next
    End Sub
    Private Sub IntersectionWallIntesectionStudRemovel()
        Dim data = MainForm.inputData
        For IntersectionWall = 0 To data.NumberofIntersectionWall - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If data.StudsPointAX(i) > data.IntersectionWallAX(IntersectionWall) And data.IntersectionWallcX(IntersectionWall) > data.StudsPointCX(i) And (data.IntersectionWallcy(IntersectionWall) = data.StudsPointCY(i)) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                        removedStuds += 1
                    End If
                End If
            Next
        Next IntersectionWall
        ''''''''''''''''''''''''''''''''''''

        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If data.StudsPointCX(i) >= data.IntersectionWallAX(IntersectionWall) And data.IntersectionWallAX(IntersectionWall) >= data.StudsPointAX(i) And (data.IntersectionWallcy(IntersectionWall) = data.StudsPointCY(i)) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next IntersectionWall

        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If data.StudsPointAX(i) <= data.IntersectionWallAX(IntersectionWall) And data.IntersectionWallAX(IntersectionWall) <= data.StudsPointCX(i) And data.IntersectionWallBy(IntersectionWall) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.IntersectionWallcX(IntersectionWall) And data.IntersectionWallcX(IntersectionWall) <= data.StudsPointCX(i) And (data.IntersectionWallcy(IntersectionWall) = data.StudsPointCY(i))) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next IntersectionWall

        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If data.StudsPointAX(i) <= data.IntersectionWallcX(IntersectionWall) And data.IntersectionWallcX(IntersectionWall) <= data.StudsPointCX(i) And data.IntersectionWallBy(IntersectionWall) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.IntersectionWallcX(IntersectionWall) And data.IntersectionWallcX(IntersectionWall) <= data.StudsPointCX(i) And (data.IntersectionWallcy(IntersectionWall) = data.StudsPointCY(i))) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next IntersectionWall

        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If (data.StudsPointAX(i) = data.IntersectionWallcX(IntersectionWall)) And data.IntersectionWallcy(IntersectionWall) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.IntersectionWalldy(IntersectionWall) Then 'And data.IntersectionWallAX(IntersectionWall) <= data.StudsPointCX(i) And  Then 'And Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If 'And (data.IntersectionWallBY(IntersectionWall) = data.StudsPointBY(i)) 
                End If
            Next i
        Next IntersectionWall

        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else

                    If (data.StudsPointCX(i) = data.IntersectionWallAX(IntersectionWall)) And data.IntersectionWallcy(IntersectionWall) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.IntersectionWalldy(IntersectionWall) Then 'And data.IntersectionWallAX(IntersectionWall) <= data.StudsPointCX(i) And  Then 'And Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If 'And (data.IntersectionWallBY(IntersectionWall) = data.StudsPointBY(i)) 
                End If
            Next i
        Next IntersectionWall


        For IntersectionWall = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.IntersectionWallcX(IntersectionWall) = 0 Then

                Else
                    If (data.StudsPointCX(i) >= data.IntersectionWallAX(IntersectionWall)) And (data.StudsPointCX(i) - data.IntersectionWallAX(IntersectionWall)) < (data.StudsWidth) And data.IntersectionWallcy(IntersectionWall) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.IntersectionWalldy(IntersectionWall) Then 'And data.IntersectionWallAX(IntersectionWall) <= data.StudsPointCX(i) And  Then 'And Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next i
        Next IntersectionWall
    End Sub
    Private Sub calculateIntersectionWall()
        Dim data = MainForm.inputData

        Dim base = data.wallIntersection.Count
        For i = 0 To data.wallIntersection.Count - 1


            Dim verticalElementWidth As Integer = data.wallIntersection(i).wallWidth

            KamelElementHeightCalculation(data.wallIntersection(i).wallLocation, data.studsFlange)

            data.IntersectionWallAX(i) = data.wallIntersection(i).wallLocation - data.studsFlange
            data.IntersectionWallAY(i) = data.ClearCut

            data.IntersectionWallBX(i) = data.IntersectionWallAX(i)
            data.IntersectionWallBy(i) = WallHeightX - data.ClearCut

            data.IntersectionWallcX(i) = data.wallIntersection(i).wallLocation ''data.IntersectionWallAX(i) + data.studsFlange
            data.IntersectionWallcy(i) = data.ClearCut

            data.IntersectionWalldX(i) = data.IntersectionWallcX(i)
            data.IntersectionWalldy(i) = data.IntersectionWallBy(i)

            verticalElementWidth = data.wallIntersection(0).wallWidth

            KamelElementHeightCalculation(data.wallIntersection(i).wallLocation + data.wallIntersection(i).wallWidth, data.studsFlange)

            data.IntersectionWallAX(base + i) = data.wallIntersection(i).wallLocation - data.studsFlange + verticalElementWidth
            data.IntersectionWallAY(base + i) = data.ClearCut

            data.IntersectionWallBX(base + i) = data.IntersectionWallAX(base + i)
            data.IntersectionWallBy(base + i) = WallHeightX - data.ClearCut

            data.IntersectionWallcX(base + i) = data.IntersectionWallAX(base + i) + data.studsFlange
            data.IntersectionWallcy(base + i) = data.ClearCut

            data.IntersectionWalldX(base + i) = data.IntersectionWallcX(base + i)
            data.IntersectionWalldy(base + i) = data.IntersectionWallBy(base + i)

            base += 1
        Next

        data.NumberofIntersectionWall = base
    End Sub

    Private Sub OpeningSilHeight(data As InputData, i As Integer, xRatio As Double, yRatio As Double, leftX As Double, sil As Double, open_Width As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim openWidth = open_Width * xRatio
        Dim sill = offsetHeight - (sil * yRatio)
        Dim openingWidth = margin + (openWidth / (i + 1)) + (leftX * xRatio)
        Dim xSpace = 1.5 * xRatio
        Dim ySpace = 1.5 * yRatio

        For i = 0 To verticalElement_temp - 1
            If (data.CXX(i) = leftX) And data.VERTICALMEMBER(i) = "Jambs" Then
                Dim move = i
                While (data.AXX(move) - leftX) < 4.5
again:
                    move += 1
                End While

                If data.VERTICALMEMBER(move) = "Jambs" Or data.VERTICALMEMBER(move) = "Studs" Then
                    openingWidth = margin + ((data.CXX(i) + (data.AXX(move) - leftX) / 2)) * xRatio
                    GoTo nextStep
                End If

                GoTo again

            End If
        Next
nextStep:
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, openingWidth, offsetHeight, openingWidth, sill))
        uiWindow.g.DrawLabel(uiWindow.CanvasArea, sil, 10, Brushes.Blue, openingWidth - 10, (offsetHeight + sill) / 2)

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, openingWidth + xSpace, offsetHeight - ySpace, openingWidth - xSpace, offsetHeight + ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, openingWidth + xSpace, sill - ySpace, openingWidth - xSpace, sill + ySpace))

    End Sub


    Private Sub OpeningLowerWidth(i As Integer, xRatio As Double, yRatio As Double, left As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim offset_Height = OffsetY_Y + 4 * xRatio + i
        Dim LeftXBoundary = left * xRatio + margin
        Dim ylbl2 As String = left

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, margin, offset_Height, LeftXBoundary, offset_Height)) ' opening - X-axis
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Pink, LeftXBoundary, offsetHeight, LeftXBoundary, offset_Height)) ' extra feature

        uiWindow.g.DrawLabel(uiWindow.CanvasArea, ylbl2, 10, Brushes.Blue, (LeftXBoundary / 2), offset_Height - 12)

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, margin, offset_Height - yRatio, margin, offset_Height + yRatio)) ' straight - left
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, margin - xRatio, offset_Height + yRatio, margin + xRatio, offset_Height - yRatio)) ' left-slant

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, LeftXBoundary, offset_Height - yRatio, LeftXBoundary, offset_Height + yRatio)) 'straight - right
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, LeftXBoundary - xRatio, offset_Height + yRatio, LeftXBoundary + xRatio, offset_Height - yRatio)) ' right- slant
    End Sub
    Private Sub DrawOpeningDimensions(xRatio As Double, yRatio As Double, leftX As Double, sil As Double, open_Height As Double, open_Width As Double, OffsetY_Y As Double)
        Dim margin = 50
        Dim offsetHeight = OffsetY_Y
        Dim openHeight = open_Height * yRatio
        Dim openWidth = open_Width * xRatio
        Dim LeftXBoundary = leftX * xRatio + margin

        Dim sill = offsetHeight - (sil * yRatio)
        Dim Xlbl As String = open_Height
        Dim ylbl As String = open_Width
        Dim xSpace = 2 * xRatio
        Dim ySpace = 2 * yRatio

        Dim openingWidth = margin + (openWidth / 4) + (leftX * xRatio)
        Dim openingHeight = offsetHeight - (((openHeight / 2)) + (sil * yRatio))
        Dim yend = offsetHeight - ((openHeight) + (sil * yRatio))


        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, LeftXBoundary, openingHeight, LeftXBoundary + openWidth, openingHeight)) ' opening - width
        uiWindow.g.DrawLabel(uiWindow.CanvasArea, ylbl, 10, Brushes.Blue, LeftXBoundary + (openWidth / 2), openingHeight - 10)

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.DarkCyan, openingWidth, sill, openingWidth, yend))  ' Opening - height
        uiWindow.g.DrawLabel(uiWindow.CanvasArea, Xlbl, 10, Brushes.Blue, openingWidth - 10, sill - (openHeight / 2.5))

        '  draw crosses
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, LeftXBoundary, openingHeight - ySpace, LeftXBoundary, openingHeight + ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, LeftXBoundary - xSpace, openingHeight + ySpace, LeftXBoundary + xSpace, openingHeight - ySpace))

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, LeftXBoundary + openWidth, openingHeight - ySpace, LeftXBoundary + openWidth, openingHeight + ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, LeftXBoundary + openWidth - xSpace, openingHeight + ySpace, LeftXBoundary + openWidth + xSpace, openingHeight - ySpace))

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, openingWidth - xSpace, sill, openingWidth + xSpace, sill))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, openingWidth - xSpace, sill + ySpace, openingWidth + xSpace, sill - ySpace))

        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, openingWidth + xSpace, yend, openingWidth - xSpace, yend))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Purple, openingWidth - xSpace, yend + ySpace, openingWidth + xSpace, yend - ySpace))
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
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, margin, yPoint, x, yPoint)) ' X-axis
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, margin / 2, offsetHeight, margin / 2, yend)) ' Y-axis
        uiWindow.g.DrawLabel(uiWindow.CanvasArea, Xlbl, 12, Brushes.Black, x / 2, yPoint - 14)
        uiWindow.g.DrawLabel(uiWindow.CanvasArea, ylbl, 12, Brushes.Black, margin / 2 - 10, y)
        '  draw crosses
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, margin, yPoint - ySpace, margin, yPoint + ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, margin - xSpace, yPoint + ySpace, margin + xSpace, yPoint - ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, x, yPoint - ySpace, x, yPoint + ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, x - xSpace, yPoint + ySpace, x + xSpace, yPoint - ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, (margin / 2) - xSpace, offsetHeight, (margin / 2) + xSpace, (offsetHeight)))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, (margin / 2) - xSpace, offsetHeight + ySpace, (margin / 2) + xSpace, (offsetHeight) - ySpace))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, (margin / 2) - xSpace, yend, (margin / 2) + xSpace, yend))
        uiWindow.CanvasArea.Children.Add(uiWindow.g.DrawLine(Brushes.Red, (margin / 2) - xSpace, yend + ySpace, (margin / 2) + xSpace, yend - ySpace))
    End Sub
    Private Sub DrawingHorizontalBlocking(RatioX As Double, Ratioy As Double, OffsetX_X As Double, Offsety_y As Double, g As Graphics)
        Dim data = MainForm.inputData
        For index = 0 To 1024
            data.HorizontalBlockingAX(index) = 0
            data.HorizontalBlockingAY(index) = 0
            data.HorizontalBlockingBX(index) = 0
            data.HorizontalBlockingBY(index) = 0
            data.HorizontalBlockingCX(index) = 0
            data.HorizontalBlockingCY(index) = 0
            data.HorizontalBlockingDX(index) = 0
            data.HorizontalBlockingDY(index) = 0
        Next
        Dim position = 0
        For i = 1 To data.NumberOfBlockingElement
            Dim HeightWall = data.BlockingHeight(i)
            Dim blokingStudHeight = data.WidthofHorizontalBlocing(i)
            VerticalElementsAssemble(HeightWall)
            Dim pos = 0
            Dim type = data.BlockingType(i)
            While pos < VerticalElement
                data.HorizontalBlockingMemeberType(position) = type
                If data.CXX(pos) = data.AXX(pos + 1) Then
                    pos += 1
                    Continue While
                End If
                If data.CXX(pos) <> 0.00000 Then
                    Dim cut = IIf((data.CXX(pos) - data.AXX(pos)) > 0, data.CXX(pos) - data.AXX(pos), data.AXX(pos) - data.CXX(pos))
                    If cut = data.studsFlange Then
                        cut *= 0.5
                    ElseIf cut > data.studsFlange Then
                        cut *= 0.15
                    End If
                    If type = "Horizontal" Then
                        cut = 0
                    End If
                    data.HorizontalBlockingAX(position) = data.CXX(pos) - cut
                    data.HorizontalBlockingBX(position) = data.HorizontalBlockingAX(position)
                    While data.CXX(pos + 1) = 0
                        pos += 1
                    End While
                    cut = IIf((data.CXX(pos + 1) - data.AXX(pos + 1)) > 0, data.CXX(pos + 1) - data.AXX(pos + 1), data.AXX(pos + 1) - data.CXX(pos + 1)) '((data.CXX(pos + 1) - data.AXX(pos + 1)))
                    If cut = data.studsFlange Then
                        cut *= 0.5
                    ElseIf cut > data.studsFlange Then
                        cut *= 0.85
                    End If
                    If (data.CXX(pos + 1) - data.AXX(pos + 1)) < 0 Then
                        data.HorizontalBlockingCX(position) = data.AXX(pos + 1) - cut
                    Else
                        data.HorizontalBlockingCX(position) = data.CXX(pos + 1) - cut
                    End If
                    If type = "Horizontal" Then
                        data.HorizontalBlockingCX(position) = IIf(data.AXX(pos + 1) > data.CXX(pos + 1), data.CXX(pos + 1), data.AXX(pos + 1)) 'data.AXX(pos + 1)
                    End If
                    data.HorizontalBlockingDX(position) = data.HorizontalBlockingCX(position)
                    data.HorizontalBlockingAY(position) = HeightWall
                    data.HorizontalBlockingBY(position) = HeightWall + blokingStudHeight
                    data.HorizontalBlockingCY(position) = HeightWall
                    data.HorizontalBlockingDY(position) = data.HorizontalBlockingBY(position)
                    position += 1
                End If
                pos += 1
            End While
            Dim adjust = 0
            If type = "Vertical" Then
                adjust = 1
            End If
            'check opening intersections
            For index = 0 To position
                For k = 0 To data.numberOfOpenings - 1 '1 last empty line(grid create empty line in the end of last empty ) + 1 starting from zero 
                    Dim cut = (data.HorizontalBlockingCX(index) - data.HorizontalBlockingAX(index))
                    If cut = data.studsFlange Then
                        cut *= 0.5
                    ElseIf cut > data.studsFlange Then
                        cut *= 0.85
                    End If

                    If (data.LeftXBoundary(k) - adjust * cut) <= data.HorizontalBlockingAX(index) And data.RightXboundary(k) >= data.HorizontalBlockingAX(index) And (data.HorizontalBlockingAY(index) <= (data.HeightOpening(k) + data.SillOpening(k)) And (data.HorizontalBlockingAY(index) >= data.SillOpening(k))) Or data.LeftXBoundary(k) <= data.HorizontalBlockingCX(index) And data.RightXboundary(k) > data.HorizontalBlockingCX(index) And (data.HorizontalBlockingAY(index) <= (data.HeightOpening(k) + data.SillOpening(k)) And (data.HorizontalBlockingAY(index) >= data.SillOpening(k))) Then
                        data.HorizontalBlockingAX(index) = 0
                        data.HorizontalBlockingAY(index) = 0
                        data.HorizontalBlockingBX(index) = 0
                        data.HorizontalBlockingBY(index) = 0
                        data.HorizontalBlockingCX(index) = 0
                        data.HorizontalBlockingCY(index) = 0
                        data.HorizontalBlockingDX(index) = 0
                        data.HorizontalBlockingDY(index) = 0
                    End If
                Next
            Next
        Next

        For index As Integer = 0 To position - 1
            Dim a As Point = New Point(data.HorizontalBlockingAX(index) * RatioX + OffsetX_X, Offsety_y - data.HorizontalBlockingAY(index) * Ratioy)
            Dim b As Point = New Point(data.HorizontalBlockingBX(index) * RatioX + OffsetX_X, Offsety_y - data.HorizontalBlockingBY(index) * Ratioy)
            Dim c As Point = New Point(data.HorizontalBlockingCX(index) * RatioX + OffsetX_X, Offsety_y - data.HorizontalBlockingCY(index) * Ratioy)
            Dim d As Point = New Point(data.HorizontalBlockingDX(index) * RatioX + OffsetX_X, Offsety_y - data.HorizontalBlockingDY(index) * Ratioy)
            'B----D
            '|    |
            'A -- C
            g.DrawLine(Brushes.CornflowerBlue, b, d) 'Line BD
            g.DrawLine(Brushes.CornflowerBlue, a, c) 'Line AC
            g.DrawLine(Brushes.CornflowerBlue, a, b) 'Line AB
            g.DrawLine(Brushes.CornflowerBlue, c, d) 'Line CD
        Next

        hblockingNumber = position
    End Sub

    Dim hblockingNumber As Integer
    Sub DrawTopTrack(G As Graphics, RatioX As Double, RatioY As Double, OffsetX_X As Double, OffsetY_Y As Double)
        Dim data = MainForm.inputData
        Dim a As Point
        Dim b As Point
        Dim c As Point
        Dim d As Point
        'llllllllllllllllllllllltrack tracktrackllll
        For ss = 0 To data.NumberOfSlope - 1

            Dim TopTrackAX = data.StartX(ss)
            Dim TopTrackAY = data.HeightX(ss) - data.trackFlange
            Dim TopTrackBX = TopTrackAX
            Dim TopTrackBY = data.HeightX(ss)
            Dim TopTrackCX = data.EndX(ss)

            WallHeightX = data.HeightX(ss) + (data.EndX(ss) - data.StartX(ss)) * data.SlopX(ss)
            Dim TopTrackCY = WallHeightX - data.trackFlange
            Dim TopTrackDX = TopTrackCX
            Dim TopTrackDY = WallHeightX
            topbktrackData.AX(ss) = TopTrackAX
            topbktrackData.AY(ss) = TopTrackAY
            topbktrackData.BX(ss) = TopTrackBX
            topbktrackData.BY(ss) = TopTrackBY
            topbktrackData.CX(ss) = TopTrackCX
            topbktrackData.CY(ss) = TopTrackCY
            topbktrackData.DX(ss) = TopTrackDX
            topbktrackData.DY(ss) = TopTrackDY

            a = New Point(TopTrackAX * RatioX + OffsetX_X, OffsetY_Y - TopTrackAY * RatioY)
            b = New Point(TopTrackBX * RatioX + OffsetX_X, OffsetY_Y - TopTrackBY * RatioY)
            c = New Point(TopTrackCX * RatioX + OffsetX_X, OffsetY_Y - TopTrackCY * RatioY)
            d = New Point(TopTrackDX * RatioX + OffsetX_X, OffsetY_Y - TopTrackDY * RatioY)
            'B----D
            '|    |
            'A -- C
            G.DrawLine(Brushes.Green, b, d) 'Line BD
            G.DrawLine(Brushes.Green, a, c) 'Line AC
            G.DrawLine(Brushes.Green, a, b) 'Line AB
            G.DrawLine(Brushes.Green, c, d) 'Line CD

        Next ss

        Dim nw = 0
        Dim offset = data.NumberOfSlope
        Dim switchOut = False
        For pos = 0 To data.NumberOfSlope - 1
out:
            For move = pos + 1 To data.NumberOfSlope - 1

                If topbktrackData.DY(pos) < topbktrackData.AY(move) Or topbktrackData.DY(pos) > topbktrackData.BY(move) Then
                    topbktrackData.AX(offset + nw) = topbktrackData.CX(pos)
                    topbktrackData.AY(offset + nw) = topbktrackData.CY(pos)
                    topbktrackData.BX(offset + nw) = topbktrackData.BX(move)
                    topbktrackData.BY(offset + nw) = topbktrackData.BY(move)

                    topbktrackData.CX(offset + nw) = topbktrackData.AX(offset + nw) + data.trackFlange
                    topbktrackData.CY(offset + nw) = topbktrackData.AY(offset + nw)
                    topbktrackData.DX(offset + nw) = topbktrackData.BX(move) + data.trackFlange
                    topbktrackData.DY(offset + nw) = topbktrackData.BY(move)
                    nw += 1
                    pos += 1
                    GoTo out
                End If
            Next
        Next

        data.NumberOfSlope += nw

        For i = offset To data.NumberOfSlope - 1

            a = New Point(topbktrackData.AX(i) * RatioX + OffsetX_X, OffsetY_Y - topbktrackData.AY(i) * RatioY)
            b = New Point(topbktrackData.BX(i) * RatioX + OffsetX_X, OffsetY_Y - topbktrackData.BY(i) * RatioY)
            c = New Point(topbktrackData.CX(i) * RatioX + OffsetX_X, OffsetY_Y - topbktrackData.CY(i) * RatioY)
            d = New Point(topbktrackData.DX(i) * RatioX + OffsetX_X, OffsetY_Y - topbktrackData.DY(i) * RatioY)

            'B----D
            '|    |
            'A -- C
            G.DrawLine(Brushes.Green, b, d) 'Line BD
            G.DrawLine(Brushes.Green, a, c) 'Line AC
            G.DrawLine(Brushes.Green, a, b) 'Line AB
            G.DrawLine(Brushes.Green, c, d) 'Line CD
        Next

        Dim BottomTrackAX = 0
        Dim BottomTrackAY = 0
        Dim BottomTrackBX = 0
        Dim BottomTrackBY = data.trackFlange
        Dim BottomTrackCX = data.wallLength
        Dim BottomTrackCY = 0
        Dim BottomTrackDX = data.wallLength
        Dim BottomTrackDY = data.trackFlange

        topbktrackData.BottomTrackAX = 0
        topbktrackData.BottomTrackAY = 0
        topbktrackData.BottomTrackBX = 0
        topbktrackData.BottomTrackBY = data.trackFlange
        topbktrackData.BottomTrackCX = data.wallLength
        topbktrackData.BottomTrackCY = 0
        topbktrackData.BottomTrackDX = data.wallLength
        topbktrackData.BottomTrackDY = data.trackFlange

        a = New Point(BottomTrackAX * RatioX + OffsetX_X, OffsetY_Y - BottomTrackAY * RatioY)
        b = New Point(BottomTrackBX * RatioX + OffsetX_X, OffsetY_Y - BottomTrackBY * RatioY)
        c = New Point(BottomTrackCX * RatioX + OffsetX_X, OffsetY_Y - BottomTrackCY * RatioY)
        d = New Point(BottomTrackDX * RatioX + OffsetX_X, OffsetY_Y - BottomTrackDY * RatioY)
        'B----D
        '|    |
        'A -- C
        G.DrawLine(Brushes.Green, b, d) 'Line BD
        G.DrawLine(Brushes.Green, a, c) 'Line AC
        G.DrawLine(Brushes.Green, a, b) 'Line AB
        G.DrawLine(Brushes.Green, c, d) 'Line CD
    End Sub

    Private Sub CalculateHeader()
        Dim data = MainForm.inputData
        TotalNumberOfmember = TotalNumberOfmember + data.NumberOfOpening

        For i = 1 To data.NumberOfOpening
            If data.Header(i - 1) > 0 Then
                data.OpeningAX(i) = data.LeftXBoundary(i - 1)
                data.OpeningAY(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1)
                data.OpeningBX(i) = data.LeftXBoundary(i - 1)
                ' data.OpeningBY(i) = data.HeightOpening(i - 1) + data.SillOpening(i - 1) - data.Header(i - 1)
                data.OpeningBY(i) = data.HeightOpening(i - 1) + data.SillOpening(i - 1) + data.Header(i - 1)

                data.OpeningCX(i) = data.RightXboundary(i - 1)
                data.OpeningCY(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1)
                data.OpeningDX(i) = data.RightXboundary(i - 1)
                ' data.OpeningDY(i) = data.HeightOpening(i - 1) + data.SillOpening(i - 1) - data.Header(i - 1)
                data.OpeningDY(i) = data.HeightOpening(i - 1) + data.SillOpening(i - 1) + data.Header(i - 1)
                data.OpeningLength(i) = data.RightXboundary(i - 1) - data.LeftXBoundary(i - 1)
                data.NumberOfHeaders += 1
            Else

                data.OpeningAX(i) = 0
                data.OpeningAY(i) = 0
                data.OpeningBX(i) = 0
                data.OpeningBY(i) = 0

                data.OpeningCX(i) = 0
                data.OpeningCY(i) = 0
                data.OpeningDX(i) = 0
                data.OpeningDY(i) = 0
                data.OpeningLength(i) = 0
            End If
        Next i
    End Sub

    Private Sub CheckBackingRightStudsHeight(i As Integer, TotalNumberOfpost As Integer)
        Dim data = MainForm.inputData

        data.BackingRightStudsAx(i) = 0
        data.BackingRightStudsAy(i) = 0

        data.BackingRightStudsBx(i) = 0
        data.BackingRightStudsBy(i) = 0

        data.BackingRightStudsCx(i) = 0
        data.BackingRightStudsCy(i) = 0

        data.BackingRightStudsDx(i) = 0
        data.BackingRightStudsDy(i) = 0

        data.BackingRightStuds(i) = TotalNumberOfpost + i + data.NumberofIntersectionWall
        data.BackingRightStudsLength(i) = 0
    End Sub
    Private Sub CheckStudsHeight(Post As Integer, i As Integer)
        Dim data = MainForm.inputData
        If data.PostBy(Post) = data.StudsPointBY(i) Then

            data.StudsPointAX(i) = 0
            data.StudsPointAY(i) = 0

            data.StudsPointBX(i) = 0
            data.StudsPointBY(i) = 0

            data.StudsPointCX(i) = 0
            data.StudsPointCY(i) = 0

            data.StudsPointDX(i) = 0
            data.StudsPointDY(i) = 0
        End If
        Return

    End Sub

    Private Sub BackingRightStud()
        ' check backing right studs with intermediat post to bde removed
        Dim data = MainForm.inputData
        Dim interPost = data.NumberOfInteriorPost - 1
        Dim TotalNumberOfpost = data.NumberOfOpening * 2 + data.endOfPost * 2 + interPost + data.NumberofIntersectionWall * 2
        For i = 1 To data.NumberofIntersectionWall
            For Ip = 1 To data.NumberOfInteriorPost - 1
                If data.BackingRightStudsCx(i) >= data.InterMetdiatPostAX(Ip) And data.BackingRightStudsCx(i) <= data.InterMetdiatPostCX(Ip) Then
                    CheckBackingRightStudsHeight(i, TotalNumberOfpost)
                End If
                If data.BackingRightStudsAx(i) >= data.InterMetdiatPostAX(Ip) And data.dataBackingRightStudsCx(i) <= data.InterMetdiatPostCX(Ip) Then
                    CheckBackingRightStudsHeight(i, TotalNumberOfpost)
                End If
                If data.BackingRightStudsAx(i) >= data.InterMetdiatPostAX(Ip) And data.BackingRightStudsAx(i) <= data.InterMetdiatPostCX(Ip) Then
                    CheckBackingRightStudsHeight(i, TotalNumberOfpost)
                End If
            Next Ip
        Next i

        Dim Offset = 2 * (data.endOfPost)
        For Post As Integer = 1 To 2 * (data.endOfPost)

            data.PostAx(Post) = data.EndPostStXA(Post)
            data.PostAy(Post) = data.EndPostStYA(Post)
            data.PostBx(Post) = data.EndPostStXB(Post)
            data.PostBy(Post) = data.EndPostStYB(Post)
            data.PostCx(Post) = data.EndPostStXC(Post)
            data.PostCy(Post) = data.EndPostStYC(Post)
            data.PostDx(Post) = data.EndPostStXD(Post)
            data.PostDy(Post) = data.EndPostStYD(Post)
        Next Post



        TotalNumberOfmember = TotalNumberOfpost + data.NumTotalNumberoFstuds

        ' creat a loop to remove all extra studs
        For Post = 1 To TotalNumberOfpost
            For i = 1 To data.NumTotalNumberoFstuds

                If data.StudsPointCX(i) >= data.PostAx(Post) And data.StudsPointCX(i) <= data.PostCx(Post) Then
                    CheckStudsHeight(Post, i)
                End If

                If data.StudsPointAX(i) >= data.PostAx(Post) And data.StudsPointCX(i) <= data.PostCx(Post) Then
                    'GoSub CheckStudsHeight
                    CheckStudsHeight(Post, i)
                End If

                If data.StudsPointAX(i) >= data.PostAx(Post) And data.StudsPointAX(i) <= data.PostCx(Post) Then
                    CheckStudsHeight(Post, i)
                End If

                If data.StudsPointCX(i) >= data.PostCx(Post) And data.StudsPointCX(i) <= data.PostAx(Post) Then
                    CheckStudsHeight(Post, i)
                End If

                If data.StudsPointAX(i) >= data.PostCx(Post) And data.StudsPointCX(i) <= data.PostAx(Post) Then
                    CheckStudsHeight(Post, i)
                End If

                If data.StudsPointAX(i) >= data.PostCx(Post) And data.StudsPointAX(i) <= data.PostAx(Post) Then
                    CheckStudsHeight(Post, i)
                End If
            Next i

        Next Post

        ' check backing right studs with intermediat post to bde removed

        For i = 1 To data.NumberofIntersectionWall
            For Ip = 1 To interPost
                If data.BackingRightStudsCx(i) >= data.InterMetdiatPostAX(Ip) And data.BackingRightStudsCx(i) <= data.InterMetdiatPostCX(Ip) Then CheckBackingRightStudsHeight(i, TotalNumberOfpost)

                If data.BackingRightStudsAx(i) >= data.InterMetdiatPostAX(Ip) And data.BackingRightStudsCx(i) <= data.InterMetdiatPostCX(Ip) Then
                    CheckBackingRightStudsHeight(i, TotalNumberOfpost)
                End If
                If data.BackingRightStudsAx(i) >= data.InterMetdiatPostAX(Ip) And data.BackingRightStudsAx(i) <= data.InterMetdiatPostCX(Ip) Then CheckBackingRightStudsHeight(i, TotalNumberOfpost)
            Next Ip
        Next i



        Return
    End Sub
    Private Sub CheckLeftCut(i As Integer, j As Integer)
        Dim data = MainForm.inputData
        If data.SillOpening(i) > data.SillOpening(j) Then CutJambLeftlower(i, j)
        If data.SillOpening(i) < data.SillOpening(j) Then CutJambLeftUpper(i, j)
    End Sub

    Private Sub CutJambLeftlower(i As Integer, j As Integer)
        Dim data = MainForm.inputData
        If LowerLeftJambBoundary > data.SillOpening(j) + data.HeightOpening(j) Then Return
        data.JambLeftYA(i) = data.HeightOpening(j) + data.SillOpening(j) + data.ClearCut + data.Header(j)
        data.JambLeftYB(i) = data.JambLeftYB(i)
        data.JambLeftXA(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i) 'jamb
        data.JambLeftXB(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i)
        data.JambLeftYC(i) = data.JambLeftYA(i)
        data.JambLeftYD(i) = data.JambLeftYB(i)
        data.JambLeftXC(i) = data.LeftXBoundary(i)
        data.JambLeftXD(i) = data.LeftXBoundary(i)
        LowerLeftJambBoundary = data.JambLeftYA(i)
#Region "unknowncode"
        'Cells(ElementNumber + i, 13) = "Cut Lower"
        'Cells(ElementNumber + i, 14) = ElementNumber + i
        'Cells(ElementNumber + i, 16) = JambLeftYA(i) : Cells(ElementNumber + i, 18) = JambLeftYB(i)
        'Cells(ElementNumber + i, 15) = JambLeftXA(i) : Cells(ElementNumber + i, 17) = JambLeftXB(i)
        'Cells(ElementNumber + i, 20) = JambLeftYC(i) : Cells(ElementNumber + i, 22) = JambLeftYD(i)
        'Cells(ElementNumber + i, 19) = JambLeftXC(i) : Cells(ElementNumber + i, 21) = JambLeftXD(i)
        'Cells(ElementNumber + i, 23) = "JambLeft"
        'JambLeftLength(i) = JambLeftYB(i) - JambLeftYA(i)
        'Cells(ElementNumber + i, 24) = JambLeftLength(i)

#End Region
        Return
    End Sub

    Private Sub CutJambLeftUpper(i As Integer, j As Integer)
        Dim data = MainForm.inputData
        If UpperLeftJambBoundary < data.SillOpening(j) Then Return
        data.JambLeftYA(i) = data.JambLeftYA(i)
        data.JambLeftYB(i) = data.SillOpening(j) - data.ClearCut
        data.JambLeftXA(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i)
        data.JambLeftXB(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i)
        data.JambLeftYC(i) = data.JambLeftYA(i)
        data.JambLeftYD(i) = data.JambLeftYB(i)
        data.JambLeftXC(i) = data.LeftXBoundary(i)
        data.JambLeftXD(i) = data.LeftXBoundary(i)
#Region "unknowncode"
        'UpperLeftJambBoundary = data.JambLeftYB(i)
        ''Cells(ElementNumber + i, 13) = "Cut Upper"
        'Cells(ElementNumber + i, 14) = ElementNumber + i
        'Cells(ElementNumber + i, 16) = JambLeftYA(i) : Cells(ElementNumber + i, 18) = JambLeftYB(i)
        'Cells(ElementNumber + i, 15) = JambLeftXA(i) : Cells(ElementNumber + i, 17) = JambLeftXB(i)
        'Cells(ElementNumber + i, 20) = JambLeftYC(i) : Cells(ElementNumber + i, 22) = JambLeftYD(i)
        'Cells(ElementNumber + i, 19) = JambLeftXC(i) : Cells(ElementNumber + i, 21) = JambLeftXD(i)
        'Cells(ElementNumber + i, 23) = "JambLeft"
        'JambLeftLength(i) = JambLeftYB(i) - JambLeftYA(i)
        'Cells(ElementNumber + i, 24) = JambLeftLength(i)
#End Region
        Return

    End Sub
    Private Sub KamelCodeDrawJamb(NumbreOfStuds As Integer)
        ElementNumber = NumbreOfStuds + n

        Dim data = MainForm.inputData
        Dim TotalNumberoFstuds = ElementNumber
        Dim JambLineStart = TotalNumberoFstuds
        If data.RightXboundary(0) = 0 And data.LeftXBoundary(0) = 0 Then
            Return
        End If
        ' jamb left
        For i = 0 To data.NumberOfOpening - 1
            Dim JaL = 1
            data.JambLeftXA(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i) * JaL
            data.JambLeftXB(i) = data.LeftXBoundary(i) - data.JambWidthLeft(i) * JaL
            Dim xx = data.JambLeftXA(i)

            Dim VerticalElementWidth = data.JambWidthLeft(i)

            KamelElementHeightCalculation(xx, VerticalElementWidth)

            data.JambLeftYA(i) = data.ClearCut
            data.JambLeftYB(i) = WallHeightX - data.ClearCut
            data.JambLeftYC(i) = data.JambLeftYA(i)
            data.JambLeftYD(i) = data.JambLeftYB(i)

            data.JambLeftXC(i) = data.LeftXBoundary(i)
            data.JambLeftXD(i) = data.JambLeftXC(i)
            data.JambLeftXC(i) = data.JambLeftXA(i) + data.JambWidthLeft(i) * JaL
            data.JambLeftXD(i) = data.JambLeftXC(i)
            data.JambLeftLength(i) = data.JambLeftYB(i) - data.JambLeftYA(i)

            VerticalElement = VerticalElement + 1

        Next i

        ' left jamb cut
        For i = 0 To data.NumberOfOpening - 1
            LowerLeftJambBoundary = data.JambLeftYA(i)
            UpperLeftJambBoundary = data.JambLeftYB(i)

            For j = 0 To data.NumberOfOpening - 1
                If data.LeftXBoundary(i) > data.LeftXBoundary(j) And data.LeftXBoundary(i) < data.RightXboundary(j) Then
                    CheckLeftCut(i, j)
                End If
            Next j
        Next i

        ElementNumber = ElementNumber + data.NumberOfOpening
        '----------------------------------------------------------------------
        ' jamb Right
        For i = 0 To data.NumberOfOpening - 1
            data.JambRightXA(i) = data.RightXboundary(i)
            data.JambRightXB(i) = data.RightXboundary(i)

            Dim VerticalElementWidth = data.JambWidthRight(i)
            Dim xx = data.JambRightXA(i)
            KamelElementHeightCalculation(xx, VerticalElementWidth)

            data.JambRightYA(i) = data.ClearCut
            data.JambRightYB(i) = WallHeightX - data.ClearCut

            data.JambRightYC(i) = data.JambRightYA(i)
            data.JambRightYD(i) = data.JambRightYB(i)

            data.JambRightXC(i) = data.RightXboundary(i) + data.JambWidthRight(i)
            data.JambRightXD(i) = data.JambRightXC(i)

            data.JambRightLength(i) = data.JambRightYB(i) - data.JambRightYA(i)
            VerticalElement = VerticalElement + 1
        Next i

        TotalNumberOfElement = ElementNumber + data.NumberOfOpening
        ' jamb cut right

        For i = 0 To data.NumberOfOpening - 1
            LowerRightJambBoundary = data.JambRightYA(i)
            UpperRightJambBoundary = data.JambRightYB(i)

            For j = 0 To data.NumberOfOpening - 1
                If data.RightXboundary(i) < data.RightXboundary(j) And data.RightXboundary(i) > data.LeftXBoundary(j) Then


                    CheckRightCut(i, j)
                End If
            Next j
        Next i
    End Sub

    Private Sub CutInterMetdiatPostLOwer(ip As Integer, i As Integer, JambWidth As Double, base As Integer)
        Dim data = MainForm.inputData
        Dim prevBy = data.InterMetdiatPostBY(ip)

        'create new element 
        base = 0

        data.InterMetdiatPostAX(ip + base) = data.InterMetdiatPostAX(ip)
        data.InterMetdiatPostAY(ip + base) = data.SillOpening(i) + data.HeightOpening(i) + data.ClearCut + data.Header(i)

        data.InterMetdiatPostBX(ip + base) = data.InterMetdiatPostAX(ip)
        data.InterMetdiatPostBY(ip + base) = prevBy

        data.InterMetdiatPostCX(ip + base) = data.InterMetdiatPostCX(ip)
        data.InterMetdiatPostCY(ip + base) = data.InterMetdiatPostAY(ip + base)

        data.InterMetdiatPostDX(ip + base) = data.InterMetdiatPostDX(ip)
        data.InterMetdiatPostDY(ip + base) = prevBy
    End Sub

    Private Sub KamelCodeInterPost()
        '  inter post
        Dim data = MainForm.inputData
        Dim EndPOst = data.endOfPost
        TotalNumberOfElement = TotalNumberOfElement + 2 * EndPOst
        Dim Jambwidth = data.endPostWidth


        Dim InterPost = data.NumberOfInteriorPost
        numberOfInterMidPost = InterPost

        For Ip = 1 To InterPost

            data.InterMetdiatPostAY(Ip) = data.ClearCut

            Dim VerticalElementWidth = data.InteriorPostWidth(Ip)

            data.InterMetdiatPostBX(Ip) = data.InterMetdiatPostAX(Ip) 'Cells(39 + Ip, 2)

            Dim xx = data.InterMetdiatPostAX(Ip)

            KamelElementHeightCalculation(xx, VerticalElementWidth)

            data.InterMetdiatPostBY(Ip) = WallHeightX - data.ClearCut

            data.InterMetdiatPostCX(Ip) = data.InterMetdiatPostAX(Ip) + data.InteriorPostWidth(Ip) 'Cells(39 + Ip, 2) + 

            data.InterMetdiatPostCY(Ip) = data.ClearCut

            data.InterMetdiatPostDX(Ip) = data.InterMetdiatPostAX(Ip) + data.InteriorPostWidth(Ip) 'Cells(39 + Ip, 2) + 
            data.InterMetdiatPostDY(Ip) = WallHeightX - data.ClearCut

            VerticalElement = VerticalElement + 1

        Next Ip
        ' CUT INTER POST
        Dim newElements As Integer = 0
        For i = 0 To data.numberOfOpenings - 1
            For Ip = 1 To InterPost + newElements
                If data.InterMetdiatPostAX(Ip) >= data.LeftXBoundary(i) And data.InterMetdiatPostCX(Ip) <= data.RightXboundary(i) Then
                    If data.InterMetdiatPostAY(Ip) <= data.SillOpening(i) + data.HeightOpening(i) Then


                        CutInterMetdiatPostLOwer(Ip, i, Jambwidth, newElements)
                    End If

                End If
            Next Ip
        Next i
        numberOfInterMidPost = InterPost
        data.NumberOfInteriorPost = InterPost
        Dim TotalNumberOfpost = data.NumberOfOpening * 2 + EndPOst * 2


        Dim NumberofIntersectionWall = 0
        If NumberofIntersectionWall > 0 Then

            For i = 1 To NumberofIntersectionWall

                data.BackingStudsAx(i) = 0
                data.WallBackingThickness(i) = 0
            Next i
        End If
    End Sub

    Private Sub RemoveJambI(i As Integer, j As Integer)
        Dim Data = MainForm.inputData

        Data.JambAX(i) = 0
        Data.JambAY(i) = 0

        Data.JambBX(i) = 0
        Data.JambBY(i) = 0

        Data.JambCX(i) = 0
        Data.JambCY(i) = 0

        Data.JambDX(i) = 0
        Data.JambDY(i) = 0

        Data.JambName$(i) = "Out"
        Data.JambLength(i) = 0

#Region "excel code"

        'Cells(JambLineStart + i, 15) = JambAX(i)
        'Cells(JambLineStart + i, 16) = JambAY(i)
        'Cells(JambLineStart + i, 17) = JambBX(i)
        'Cells(JambLineStart + i, 18) = JambBY(i)
        'Cells(JambLineStart + i, 19) = JambCX(i)
        'Cells(JambLineStart + i, 20) = JambCY(i)
        'Cells(JambLineStart + i, 21) = JambDX(i)
        'Cells(JambLineStart + i, 22) = JambDY(i)
        'Cells(JambLineStart + i, 23) = JambName$(i)
        'Cells(JambLineStart + i, 24) = JambLength(i)
#End Region

        VerticalElement = VerticalElement - 1
        Return
    End Sub
    Private Sub RemoveJambJ(i As Integer, j As Integer)
        Dim data = MainForm.inputData
        data.JambAX(j) = 0
        data.JambAY(j) = 0
        data.JambBX(j) = 0
        data.JambBY(j) = 0
        data.JambCX(j) = 0
        data.JambCY(j) = 0
        data.JambDX(j) = 0
        data.JambDY(j) = 0
        data.JambName$(j) = "Out"
        data.JambLength(j) = 0
#Region "excel code"
        'Cells(JambLineStart + j, 15) = JambAX(j)
        'Cells(JambLineStart + j, 16) = JambAY(j)
        'Cells(JambLineStart + j, 17) = JambBX(j)
        'Cells(JambLineStart + j, 18) = JambBY(j)
        'Cells(JambLineStart + j, 19) = JambCX(j)
        'Cells(JambLineStart + j, 20) = JambCY(j)
        'Cells(JambLineStart + j, 21) = JambDX(j)
        'Cells(JambLineStart + j, 22) = JambDY(j)
        'Cells(JambLineStart + j, 23) = JambName$(j)
        'Cells(JambLineStart + j, 24) = JambLength(j)
#End Region
        VerticalElement = VerticalElement - 1
        Return
    End Sub
    Private Sub CheckJambHeight(i As Integer, j As Integer)
        Dim Data = MainForm.inputData
        If Data.JambLength(i) >= Data.JambLength(j) Then RemoveJambJ(i, j)

        If Data.JambLength(i) < Data.JambLength(j) Then RemoveJambI(i, j)
        Return

    End Sub
    Dim removedStuds = 0
    Private Sub RemoveIntractionJambWithStud()
        ' remove inersection jamb  remove inersection jamb remove inersection jamb remove inersection jamb remove inersection jamb remove inersection jamb

        Dim data = MainForm.inputData

        For i = 0 To 99
            data.JambAX(i) = 0
            data.JambAY(i) = 0

            data.JambBX(i) = 0
            data.JambBY(i) = 0

            data.JambCX(i) = 0
            data.JambCY(i) = 0

            data.JambDX(i) = 0
            data.JambDY(i) = 0
            data.JambLength(i) = 0
        Next

        Dim offset = 1
        For i = 0 To data.opening.Count - 1

            data.JambAX(i) = data.JambRightXA(i)
            data.JambAY(i) = data.JambRightYA(i)

            data.JambBX(i) = data.JambRightXB(i)
            data.JambBY(i) = data.JambRightYB(i)

            data.JambCX(i) = data.JambRightXC(i)
            data.JambCY(i) = data.JambRightYC(i)

            data.JambDX(i) = data.JambRightXD(i)
            data.JambDY(i) = data.JambRightYD(i)
            data.JambLength(i) = data.JambRightLength(i)
            data.ShearScrewOpening(i) = data.opening(i).shersrc
        Next i

        offset = (data.NumberOfOpening - 1) + 1
        For i = 0 To data.NumberOfOpening - 1



            data.JambAX(offset + i) = data.JambLeftXA(i)
            data.JambAY(offset + i) = data.JambLeftYA(i)

            data.JambBX(offset + i) = data.JambLeftXB(i)
            data.JambBY(offset + i) = data.JambLeftYB(i)
            data.JambCX(offset + i) = data.JambLeftXC(i)
            data.JambCY(offset + i) = data.JambLeftYC(i)

            data.JambDX(offset + i) = data.JambLeftXD(i)
            data.JambDY(offset + i) = data.JambLeftYD(i)

            data.JambLength(offset + i) = data.JambLeftLength(i)

        Next i


        Dim tempJambLeft(2000) As Double

        For i = 0 To data.opening.Count - 1
            tempJambLeft(i) = data.JambLeftXA(i)
        Next
        For i = 0 To data.NumberOfOpening - 1
            For j = i + 1 To data.NumberOfOpening - 1
                If tempJambLeft(j) < tempJambLeft(i) Then
                    tempJambLeft(i) = tempJambLeft(i) Xor tempJambLeft(j)
                    tempJambLeft(j) = tempJambLeft(i) Xor tempJambLeft(j)
                    tempJambLeft(i) = tempJambLeft(i) Xor tempJambLeft(j)

                    data.ShearScrewOpening(i) = data.ShearScrewOpening(i) Xor data.ShearScrewOpening(j)
                    data.ShearScrewOpening(j) = data.ShearScrewOpening(i) Xor data.ShearScrewOpening(j)
                    data.ShearScrewOpening(i) = data.ShearScrewOpening(i) Xor data.ShearScrewOpening(j)
                End If
            Next
        Next
        '''''''''''''''Studs positioned inside jamb case'''''''''''''''''''''

        If (data.NumberOfOpening - 1) = 0 Then
            Return
        End If

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then

                Else
                    If data.StudsPointAX(i) > data.JambAX(jamb) And data.JambCX(jamb) > data.StudsPointCX(i) And (data.JambCY(jamb) = data.StudsPointCY(i)) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                        removedStuds += 1
                    End If
                End If
            Next
        Next jamb
        ''''''''''''''''''''''''''''''''''''

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then

                Else
                    If data.StudsPointCX(i) >= data.JambAX(jamb) And data.JambAX(jamb) >= data.StudsPointAX(i) And (data.JambCY(jamb) = data.StudsPointCY(i)) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next jamb

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then

                Else
                    If data.StudsPointAX(i) <= data.JambAX(jamb) And data.JambAX(jamb) <= data.StudsPointCX(i) And data.JambBY(jamb) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.JambCX(jamb) And data.JambCX(jamb) <= data.StudsPointCX(i) And (data.JambCY(jamb) = data.StudsPointCY(i))) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next jamb

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then

                Else
                    If data.StudsPointAX(i) <= data.JambCX(jamb) And data.JambCX(jamb) <= data.StudsPointCX(i) And data.JambBY(jamb) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.JambCX(jamb) And data.JambCX(jamb) <= data.StudsPointCX(i) And (data.JambCY(jamb) = data.StudsPointCY(i))) Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next
        Next jamb

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then
                Else
                    If (data.StudsPointAX(i) = data.JambCX(jamb)) And data.JambCY(jamb) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.JambDY(jamb) Then 'And data.JambAX(jamb) <=

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next i
        Next jamb

        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then
                Else
                    If (data.StudsPointCX(i) = data.JambAX(jamb)) And data.JambCY(jamb) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.JambDY(jamb) Then 'And data.JambAX(jamb) <=
                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next i
        Next jamb


        For jamb = 0 To (2 * data.NumberOfOpening) - 1
            For i = 0 To VerticalElement + n - 1
                If data.JambCX(jamb) = 0 Then

                Else
                    If (data.StudsPointCX(i) >= data.JambAX(jamb)) And (data.StudsPointCX(i) - data.JambAX(jamb)) < (data.StudsWidth) And data.JambCY(jamb) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.JambDY(jamb) Then 'And data.JambAX(jamb) <= data.StudsPointCX(i) And  Then 'And Then

                        data.StudsPointAX(i) = 0
                        data.StudsPointAY(i) = 0

                        data.StudsPointBX(i) = 0
                        data.StudsPointBY(i) = 0

                        data.StudsPointCX(i) = 0
                        data.StudsPointCY(i) = 0

                        data.StudsPointDX(i) = 0
                        data.StudsPointDY(i) = 0
                    End If
                End If
            Next i
        Next jamb




    End Sub
    Private Sub RemoveIntractionEndPostStud()
        ' remove inersection jamb  remove inersection jamb remove inersection jamb remove inersection jamb remove inersection jamb remove inersection jamb
        Dim data = MainForm.inputData
        If data.endPostBox = False Then
            Return
        End If
        For jamb = 1 To 2 * data.endOfPost
            For i = 0 To VerticalElement + n - 1
                If data.StudsPointAX(i) >= data.EndPostStXA(jamb) And data.EndPostStXC(jamb) >= data.StudsPointCX(i) And (data.StudsPointAX(i) - data.EndPostStXA(jamb)) < data.endPostWidth Then
                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                    removedStuds += 1
                End If
            Next
        Next jamb
        For jamb = 1 To 2 * data.endOfPost
            For i = 0 To VerticalElement + n - 1

                If (data.StudsPointAX(i) = data.EndPostStXC(jamb)) Or (data.StudsPointCX(i) = data.EndPostStXA(jamb)) And data.EndPostStYC(jamb) <= data.StudsPointAY(i) And data.StudsPointAY(i) <= data.EndPostStYD(jamb) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                    removedStuds += 1
                End If
            Next i
        Next jamb


        For jamb = 1 To 2 * data.endOfPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointCX(i) >= data.EndPostStXA(jamb) And data.EndPostStXA(jamb) >= data.StudsPointAX(i) And (data.EndPostStYC(jamb) = data.StudsPointCY(i)) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next jamb

        For jamb = 1 To 2 * data.endOfPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointAX(i) <= data.EndPostStXA(jamb) And data.EndPostStXA(jamb) <= data.StudsPointCX(i) And data.EndPostStYB(jamb) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.EndPostStXC(jamb) And data.EndPostStXC(jamb) <= data.StudsPointCX(i) And (data.EndPostStYC(jamb) = data.StudsPointCY(i))) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0F
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next jamb

        For jamb = 1 To 2 * data.endOfPost
            For i = 0 To VerticalElement + n - 1

                If data.StudsPointAX(i) <= data.EndPostStXC(jamb) And data.EndPostStXC(jamb) <= data.StudsPointCX(i) And data.EndPostStYB(jamb) = data.StudsPointBY(i) Or (data.StudsPointAX(i) <= data.EndPostStXC(jamb) And data.EndPostStXC(jamb) <= data.StudsPointCX(i) And (data.EndPostStYC(jamb) = data.StudsPointCY(i))) Then

                    data.StudsPointAX(i) = 0
                    data.StudsPointAY(i) = 0

                    data.StudsPointBX(i) = 0
                    data.StudsPointBY(i) = 0

                    data.StudsPointCX(i) = 0
                    data.StudsPointCY(i) = 0

                    data.StudsPointDX(i) = 0
                    data.StudsPointDY(i) = 0
                End If
            Next
        Next jamb
    End Sub

    Private Sub DrawTopBottomTrack()
        Dim data = MainForm.inputData

        Dim ss = 0
        For ss = 1 To data.NumberOfSlope

            Dim TopTrackAX = data.StartX(ss - 1)
            Dim TopTrackAY = data.HeightX(ss - 1) - data.trackFlange
            Dim TopTrackBX = TopTrackAX
            Dim TopTrackBY = data.HeightX(ss - 1)
            Dim TopTrackCX = data.EndX(ss - 1)
            WallHeightX = data.HeightX(ss - 1) + (data.EndX(ss - 1) - data.StartX(ss - 1)) * data.SlopX(ss - 1)
            Dim dataTopTrackCY = WallHeightX - data.trackFlange


        Next ss
        TotalNumberOfmember = TotalNumberOfmember + ss
        Dim BottomTrackAX = 0
        Dim BottomTrackAY = 0
        Dim BottomTrackBX = 0
        Dim BottomTrackBY = data.trackFlange
        Dim BottomTrackCX = data.wallLength
        Dim BottomTrackCY = 0
        Dim BottomTrackDX = data.wallLength
        Dim BottomTrackDY = data.trackFlange

        For i = 1 To data.NumberOfOpening
            data.TopTrackAXO(i) = data.LeftXBoundary(i - 1)
            data.TopTrackAYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1)

            data.TopTrackBXO(i) = data.LeftXBoundary(i - 1)
            '  data.TopTrackBYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1) + data.trackFlange  
            data.TopTrackBYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1) + data.trackFlange + data.Header(i - 1)
            data.TopTrackCXO(i) = data.RightXboundary(i - 1)
            data.TopTrackCYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1)

            data.TopTrackDXO(i) = data.RightXboundary(i - 1)
            data.TopTrackDYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1) + data.trackFlange + data.Header(i - 1)
            ' data.TopTrackDYO(i) = data.SillOpening(i - 1) + data.HeightOpening(i - 1) + data.trackFlange 
        Next i
        TotalNumberOfmember = TotalNumberOfmember + data.NumberOfOpening


        For i = 0 To 49
            data.BottomTrackAXO(i) = 0
            data.BottomTrackAYO(i) = 0

            data.BottomTrackBXO(i) = 0
            data.BottomTrackBYO(i) = 0

            data.BottomTrackCXO(i) = 0
            data.BottomTrackCYO(i) = 0

            data.BottomTrackDXO(i) = 0
            data.BottomTrackDYO(i) = 0
        Next
        For i = 1 To data.NumberOfOpening
            If data.SillOpening(i - 1) > 0 Then
                data.BottomTrackAXO(i) = data.LeftXBoundary(i - 1)
                data.BottomTrackAYO(i) = data.SillOpening(i - 1)

                data.BottomTrackBXO(i) = data.LeftXBoundary(i - 1)
                data.BottomTrackBYO(i) = data.SillOpening(i - 1) - data.trackFlange

                data.BottomTrackCXO(i) = data.RightXboundary(i - 1)
                data.BottomTrackCYO(i) = data.SillOpening(i - 1)

                data.BottomTrackDXO(i) = data.RightXboundary(i - 1)
                data.BottomTrackDYO(i) = data.SillOpening(i - 1) - data.trackFlange

            End If

        Next i
    End Sub

    Private Sub CreateStartEndPost()
        ' End Post left
        Dim data = MainForm.inputData


        Dim EndPost = data.endOfPost

        Dim JambWidth = data.endPostWidth
        Dim VerticalElementWidth = JambWidth

        If EndPost > 0 And data.endPostBox = True Then

            For Ep = 1 To EndPost

                data.EndPostStXA(Ep) = JambWidth * (Ep - 1)
                data.EndPostStYA(Ep) = data.ClearCut
                data.EndPostStXB(Ep) = JambWidth * (Ep - 1)
                Dim xx = data.EndPostStXA(Ep)
                KamelElementHeightCalculation(xx, VerticalElementWidth)
                data.EndPostStYB(Ep) = WallHeightX - data.ClearCut
                data.EndPostStXC(Ep) = JambWidth * (Ep)
                data.EndPostStYC(Ep) = data.ClearCut
                data.EndPostStXD(Ep) = JambWidth * (Ep)
                data.EndPostStYD(Ep) = WallHeightX - data.ClearCut
                VerticalElement = VerticalElement + 1
            Next Ep
            'end post Right
            For Ep = 1 To EndPost
                data.EndPostStXA(Ep + EndPost) = data.wallLength - JambWidth * (Ep)
                data.EndPostStYA(Ep + EndPost) = data.ClearCut
                data.EndPostStXB(Ep + EndPost) = data.wallLength - JambWidth * (Ep)
                VerticalElementWidth = JambWidth
                Dim xx = data.EndPostStXA(Ep + EndPost)
                If Ep < EndPost + 1 Then
                    KamelElementHeightCalculation(xx, VerticalElementWidth)
                End If
                data.EndPostStYB(Ep + EndPost) = WallHeightX - data.ClearCut
                data.EndPostStXC(Ep + EndPost) = data.wallLength - JambWidth * (Ep - 1)
                data.EndPostStYC(Ep + EndPost) = data.ClearCut
                data.EndPostStXD(Ep + EndPost) = data.wallLength - JambWidth * (Ep - 1)
                data.EndPostStYD(Ep + EndPost) = WallHeightX - data.ClearCut
                Dim EndpostLengthRight = data.EndPostStYBRight(Ep + EndPost) - data.EndPostStYARight(Ep + EndPost)
                VerticalElement = VerticalElement + 1
            Next Ep
        End If
        'inter post
        TotalNumberOfElement = TotalNumberOfElement + 2 * EndPost
    End Sub
    Private Sub CheckRightCut(i As Integer, j As Integer)
        Dim data = MainForm.inputData

        If data.SillOpening(i) > data.SillOpening(j) Then

            CutJambRightlower(i, j)
        End If
        If data.SillOpening(i) < data.SillOpening(j) Then

            CutJambRightUpper(i, j)
        End If
    End Sub
    Private Sub CutJambRightlower(i As Integer, j As Integer)
        Dim data = MainForm.inputData

        If LowerRightJambBoundary > data.SillOpening(j) + data.HeightOpening(j) Then Return
        data.JambRightYA(i) = data.SillOpening(j) + data.HeightOpening(j) + data.ClearCut + data.Header(j)
        data.JambRightYB(i) = data.JambRightYB(i)
        data.JambRightXA(i) = data.RightXboundary(i)
        data.JambRightXB(i) = data.RightXboundary(i)
        data.JambRightYC(i) = data.JambRightYA(i)
        data.JambRightYD(i) = data.JambRightYB(i)
        data.JambRightXC(i) = data.RightXboundary(i) + data.JambWidthRight(i)
        data.JambRightXD(i) = data.RightXboundary(i) + data.JambWidthRight(i)
        LowerRightJambBoundary = data.JambRightYA(i)
        data.JambRightLength(i) = data.JambRightYB(i) - data.JambRightYA(i)

        Return
    End Sub
    Private Sub CutJambRightUpper(i As Integer, j As Integer)
        Dim data = MainForm.inputData

        If UpperRightJambBoundary < data.SillOpening(j) Then Return
        data.JambRightYA(i) = data.JambRightYA(i)
        data.JambRightYB(i) = data.SillOpening(j) - data.ClearCut
        data.JambRightXA(i) = data.RightXboundary(i)
        data.JambRightXB(i) = data.RightXboundary(i)
        data.JambRightYC(i) = data.JambRightYA(i)
        data.JambRightYD(i) = data.JambRightYB(i)
        data.JambRightXC(i) = data.RightXboundary(i) + data.JambWidthRight(i)
        data.JambRightXD(i) = data.RightXboundary(i) + data.JambWidthRight(i)
        UpperRightJambBoundary = data.JambRightYB(i)

        data.JambRightLength(i) = data.JambRightYB(i) - data.JambRightYA(i)
        Return
    End Sub
    Private Sub KamelElementHeightCalculation(xx As Decimal, VerticalElementWidth As Decimal)
        Dim data = MainForm.inputData
        For s = 0 To data.NumberOfSlope - 1

            If xx >= data.StartX(s) And xx < data.EndX(s) Then

                Dim WallSlope = data.SlopX(s)

                If data.SlopX(s) >= 0 Then VerticalElementWidth = 0
                If xx = data.EndX(s) Then WallSlope = data.SlopX(s + 1)
                ' We found one change in Kamel code, because of this it was not drawing correctly.
                'WallHeightX = HeightX(s) + (xx - EndX(s - 1) + VerticalElementWidth) * WallSlope
                WallHeightX = data.HeightX(s) + (xx - data.StartX(s) + VerticalElementWidth) * WallSlope

            End If

        Next s
    End Sub
    Private Function ElementHeightCalculation(xx As Integer, VerticalElementWidth As Integer) As Integer

        Dim data = MainForm.inputData

        For s = 1 To data.NumberOfSlope

            If xx >= data.StartX(s - 1) And xx < data.EndX(s - 1) Then
                Dim WallSlope = data.SlopX(s - 1)
                If data.SlopX(s) >= 0 Then VerticalElementWidth = 0
                If xx = data.EndX(s) Then WallSlope = data.SlopX(s + 1)
                WallHeightX = data.HeightX(s - 1) + (xx - data.StartX(s - 1)) * WallSlope
                Return WallHeightX
                ' End If
            End If
        Next s
        Return WallHeightX

    End Function

    Public Sub SaveDxfFile()

        If NumbreOfStuds > 0 Then
            CreateDXFFile(NumbreOfStuds)

            CreateDFXScriptFile()
        End If
        'Create DXF file for autocad 
    End Sub

    Private Sub CutStudsLOwer(st As Integer, i As Integer)
        Dim Data = MainForm.inputData
        Data.StudsPointAX(st) = Data.StudsPointAX(st)
        ' Data.StudsPointAY(st) = Data.HeightOpening(i) + Data.ClearCut

        Data.StudsPointAY(st) = Data.HeightOpening(i) + Data.ClearCut + Data.Header(i)


        Data.StudsPointBX(st) = Data.StudsPointAX(st)
        Data.StudsPointBY(st) = Data.StudsPointBY(st)


        Data.StudsLength(st) = Data.StudsPointBY(st) - Data.StudsPointAY(st)
        Data.StudsPointCX(st) = Data.StudsPointAX(st) + Data.StudsWidth

        ' Data.StudsPointCY(st) = Data.HeightOpening(i) + Data.ClearCut
        Data.StudsPointCY(st) = Data.HeightOpening(i) + Data.ClearCut + Data.Header(i)

        Data.StudsPointDX(st) = Data.StudsPointCX(st)
        Data.StudsPointDY(st) = Data.StudsPointDY(st)

        Return

    End Sub

    Private Sub SpliteIntwo(st As Integer, i As Integer, NumbreOfStuds As Integer)

        Dim Data = MainForm.inputData
        VerticalElement += 1

        Data.StudsPointAX(st) = Data.StudsPointAX(st)
        Data.StudsPointAY(st) = Data.StudsPointAY(st)

        Dim Bheight = Data.StudsPointBY(st)

        Data.StudsPointBX(st) = Data.StudsPointAX(st)
        Data.StudsPointBY(st) = Data.SillOpening(i) - Data.ClearCut

        Data.StudsLength(st) = Data.StudsPointBY(st) - Data.StudsPointAY(st)

        Data.StudsPointCX(st) = Data.StudsPointAX(st) + Data.StudsWidth
        Data.StudsPointCY(st) = Data.StudsPointCY(st)

        Data.StudsPointDX(st) = Data.StudsPointCX(st)
        Data.StudsPointDY(st) = Data.SillOpening(i) - Data.ClearCut


        n += 1

        Data.StudsPointAX(NumbreOfStuds + n) = Data.StudsPointAX(st)
        Data.StudsPointAY(NumbreOfStuds + n) = Data.SillOpening(i) + Data.HeightOpening(i) + Data.ClearCut + Data.Header(i)
        ' Data.StudsPointAY(NumbreOfStuds + n) = Data.SillOpening(i) + Data.HeightOpening(i) + Data.ClearCut

        Data.StudsPointBX(NumbreOfStuds + n) = Data.StudsPointAX(NumbreOfStuds + n)
        Data.StudsPointBY(NumbreOfStuds + n) = Bheight

        Data.StudsLength(NumbreOfStuds + n) = Data.StudsPointBY(NumbreOfStuds + n) - Data.StudsPointAY(NumbreOfStuds + n)
        Data.StudsPointCX(NumbreOfStuds + n) = Data.StudsPointAX(NumbreOfStuds + n) + Data.StudsWidth
        '  Data.StudsPointCY(NumbreOfStuds + n) = Data.SillOpening(i) + Data.HeightOpening(i) + Data.ClearCut 
        Data.StudsPointCY(NumbreOfStuds + n) = Data.SillOpening(i) + Data.HeightOpening(i) + Data.ClearCut + Data.Header(i)
        Data.StudsPointDX(NumbreOfStuds + n) = Data.StudsPointCX(NumbreOfStuds + n)
        Data.StudsPointDY(NumbreOfStuds + n) = Bheight

        Data.VERTICALMEMBER(NumbreOfStuds + n) = "Studs"


        Return
    End Sub

    'opening stud
    Private Sub openingStuds(NumbreOfStuds As Integer, verticalElement As Integer)
        Dim data = MainForm.inputData
        verticalElement = NumbreOfStuds

        For i = 0 To data.NumberOfOpening - 1
            ' i = 2
            For st = 0 To NumbreOfStuds + n

                If data.StudsPointAX(st) >= data.LeftXBoundary(i) And data.StudsPointCX(st) <= data.RightXboundary(i) Then

                    If data.SillOpening(i) = 0 And data.StudsPointAY(st) < data.SillOpening(i) + data.HeightOpening(i) Then

                        CutStudsLOwer(st, i)
                    End If

                    If data.StudsPointAY(st) <= data.SillOpening(i) And data.StudsPointBY(st) > data.SillOpening(i) + data.HeightOpening(i) Then

                        SpliteIntwo(st, i, NumbreOfStuds)
                    End If

                End If

            Next st

        Next i



#Region "comment"
        'CutStudsLOwer: 

        '        'VerticalElement = VerticalElement + 1
        '        ' Cells(st, 13) = "Cut lower" ________________________________________________________________________________________
        '        data.StudsPointAX(st) = data.StudsPointAX(st)
        '        data.StudsPointAY(st) = data.HeightOpening(i) + data.ClearCut
        '        'Cells(st, 15) = StudsPointAX(st) : Cells(st, 16) = StudsPointAY(st)
        '        data.StudsPointBX(st) = data.StudsPointAX(st)
        '        data.StudsPointBY(st) = data.StudsPointBY(st)
        '        'Cells(st, 17) = StudsPointAX(st) : Cells(st, 18) = StudsPointBY(st)
        '        data.StudsLength(st) = data.StudsPointBY(st) - data.StudsPointAY(st)
        '        data.StudsPointCX(st) = data.StudsPointAX(st) + data.StudsWidth
        '        data.StudsPointCY(st) = data.HeightOpening(i) + data.ClearCut
        '        'Cells(st, 19) = StudsPointCX(st) : Cells(st, 20) = StudsPointCY(st)
        '        data.StudsPointDX(st) = data.StudsPointCX(st)
        '        data.StudsPointDY(st) = data.StudsPointDY(st)
        '        'Cells(st, 21) = StudsPointCX(st) : Cells(st, 22) = StudsPointDY(st)
        '        'Cells(st, 23) = "Studs"
        '        'Cells(st, 24) = StudsLength(st)
        '        Return

        'SpliteIntwo:

        '        verticalElement = verticalElement + 1
        '        'Cells(st, 13) = "Cut"_________________________________________________________________________________________
        '        data.StudsPointAX(st) = data.StudsPointAX(st)
        '        data.StudsPointAY(st) = data.StudsPointAY(st)
        '        'Cells(st, 15) = StudsPointAX(st) : Cells(st, 16) = StudsPointAY(st)
        '        Dim Bheight = data.StudsPointBY(st)
        '        data.StudsPointBX(st) = data.StudsPointAX(st)
        '        data.StudsPointBY(st) = data.SillOpening(i) - data.ClearCut
        '        'Cells(st, 17) = StudsPointAX(st)
        '        'Cells(st, 18) = data.StudsPointBY(st)
        '        data.StudsLength(st) = data.StudsPointBY(st) - data.StudsPointAY(st)
        '        data.StudsPointCX(st) = data.StudsPointAX(st) + data.StudsWidth
        '        data.StudsPointCY(st) = data.StudsPointCY(st)
        '        'Cells(st, 19) = StudsPointCX(st) : Cells(st, 20) = StudsPointCY(st)
        '        data.StudsPointDX(st) = data.StudsPointCX(st)
        '        data.StudsPointDY(st) = data.SillOpening(i) - data.ClearCut
        '        'Cells(st, 21) = StudsPointCX(st)
        '        'Cells(st, 22) = StudsPointDY(st)
        '        'Cells(st, 23) = "Studs"
        '        'Cells(st, 24) = StudsLength(st)
        '        ' Return

        '        '-----------------------------------------

        '        n = n + 1

        '        'Cells(NumbreOfStuds + N, 13) = "Cut"

        '        ' NumbreOfStuds = NumbreOfStuds + N
        '        'What it is?
        '        'Cells(NumbreOfStuds + n, 14) = NumbreOfStuds + n

        '        data.StudsPointAX(NumbreOfStuds + n) = data.StudsPointAX(st)
        '        data.StudsPointAY(NumbreOfStuds + n) = data.SillOpening(i) + data.HeightOpening(i) + data.ClearCut
        '        'What is this cell in thin context?
        '        'Cells(NumbreOfStuds + n, 15) = StudsPointAX(NumbreOfStuds + n) : Cells(NumbreOfStuds + n, 16) = StudsPointAY(NumbreOfStuds + n)

        '        data.StudsPointBX(NumbreOfStuds + n) = data.StudsPointAX(NumbreOfStuds + n)
        '        data.StudsPointBY(NumbreOfStuds + n) = 0  'Bheight ?What is Bheight ?

        '        'Cells(NumbreOfStuds + n, 17) = StudsPointAX(NumbreOfStuds + n)

        '        'Cells(NumbreOfStuds + n, 18) = StudsPointBY(NumbreOfStuds + n)

        '        data.StudsLength(NumbreOfStuds + n) = data.StudsPointBY(NumbreOfStuds + n) - data.StudsPointAY(NumbreOfStuds + n)

        '        data.StudsPointCX(NumbreOfStuds + n) = data.StudsPointAX(NumbreOfStuds + n) + data.StudsWidth
        '        data.StudsPointCY(NumbreOfStuds + n) = data.SillOpening(i) + data.HeightOpening(i) + data.ClearCut

        '        'Cells(NumbreOfStuds + n, 19) = StudsPointCX(NumbreOfStuds + n) : Cells(NumbreOfStuds + n, 20) = StudsPointCY(NumbreOfStuds + n)

        '        data.StudsPointDX(NumbreOfStuds + n) = data.StudsPointCX(NumbreOfStuds + n)
        '        data.StudsPointDY(NumbreOfStuds + n) = 0 'Bheight

        '        'Cells(NumbreOfStuds + n, 21) = StudsPointCX(NumbreOfStuds + n) : Cells(NumbreOfStuds + n, 22) = StudsPointDY(NumbreOfStuds + n)

        '        'Cells(NumbreOfStuds + n, 23) = "Studs"

        '        'Cells(NumbreOfStuds + n, 24) = StudsLength(NumbreOfStuds + n)

        '        Return
#End Region
    End Sub

    Private Sub DrawJambLine(vmember As String, MyFile As StreamWriter, A As Double, b As Double, C As Double, D As Double)
        Dim lineColor = 9
        If vmember = "JambLeft" Then lineColor = 4
        If vmember = "JambRight" Then lineColor = 4
        DRAWLINE(MyFile, lineColor, A, b, C, D)
    End Sub

    Private Sub DrawEndPostLine(vmember As String, MyFile As StreamWriter, A As Double, b As Double, C As Double, D As Double)
        Dim lineColor = 9
        If vmember = "EndPostleft" Then lineColor = 2
        If vmember = "EndpostRight" Then lineColor = 0
        DRAWLINE(MyFile, lineColor, A, b, C, D)
    End Sub


    Private Sub CreateDXFFile(NumberOfStuds As Integer)
        Try
            Dim data = MainForm.inputData

            For st = 0 To totalNumberOfStuds
                data.AXX(st) = data.StudsPointAX(st)
                data.AYY(st) = data.StudsPointAY(st)
                data.BXX(st) = data.StudsPointBX(st)
                data.BYY(st) = data.StudsPointBY(st)
                data.CXX(st) = data.StudsPointCX(st)
                data.CYY(st) = data.StudsPointCY(st)
                data.DXX(st) = data.StudsPointDX(st)
                data.DYY(st) = data.StudsPointDY(st)
                data.VERTICALMEMBER(st) = "Studs"
            Next st

            Dim TotalNumberOfElement = NumberOfStuds + 1 - removedStuds


            Dim dxfFilePath = fileDxf.SaveDXFFile()
            Dim MyFile = File.CreateText(dxfFilePath)

            MyFile.WriteLine(0)
            MyFile.WriteLine("SECTION")
            MyFile.WriteLine(2)
            MyFile.WriteLine("ENTITIES")
            MyFile.WriteLine(0)


            Dim A As Decimal = 0, b As Decimal = 0, C As Decimal = 0, D As Decimal = 0
            Dim lineColor = 1


            For st As Integer = 0 To totalNumberOfStuds
                If data.StudsPointBY(st) < 1 Or data.StudsPointDY(st) < 1 Then
                    Continue For
                End If
                For j As Integer = 1 To 4

                    If j = 1 Then
                        A = data.StudsPointAX(st)
                        b = data.StudsPointAY(st)
                        C = data.StudsPointBX(st)
                        D = data.StudsPointBY(st)
                    End If

                    If j = 2 Then
                        A = data.StudsPointCX(st)
                        b = data.StudsPointCY(st)
                        C = data.StudsPointDX(st)
                        D = data.StudsPointDY(st)
                    End If

                    If j = 3 Then
                        A = data.StudsPointAX(st)
                        b = data.StudsPointAY(st)
                        C = data.StudsPointCX(st)
                        D = data.StudsPointCY(st)
                    End If

                    If j = 4 Then
                        A = data.StudsPointBX(st)
                        b = data.StudsPointBY(st)
                        C = data.StudsPointDX(st)
                        D = data.StudsPointDY(st)
                    End If

                    If data.VERTICALMEMBER(st) = "Studs" Then
                        lineColor = 1
                    Else
                        lineColor = 9
                    End If

                    If data.VERTICALMEMBER(st) = "EndPostleft" Then lineColor = 2
                    If data.VERTICALMEMBER(st) = "EndpostRight" Then lineColor = 0
                    If data.VERTICALMEMBER(st) = "TopTrack" Then lineColor = 3
                    If data.VERTICALMEMBER(st) = "JambLeft" Then lineColor = 4
                    If data.VERTICALMEMBER(st) = "JambRight" Then lineColor = 4
                    If data.VERTICALMEMBER(st) = "BottomTrack" Then lineColor = 5
                    If data.VERTICALMEMBER(st) = "Blocking H." Then lineColor = 6
                    If data.VERTICALMEMBER(st) = "Blocking V." Then lineColor = 7
                    If data.VERTICALMEMBER(st) = "Hori.Strap" Then lineColor = 8
                    If data.VERTICALMEMBER(st) = "Headerx" Then lineColor = 10
                    If data.VERTICALMEMBER(st) = "Header" Then lineColor = 10
                    If data.VERTICALMEMBER(st) = "ShearPlateLeft" Then lineColor = 11
                    If data.VERTICALMEMBER(st) = "ShearPlateRight" Then lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)

                Next j
            Next st

            Dim offset = VerticalElement + n - 1 + 1
            Dim index = 0
            For st = 0 To data.NumberOfOpening - 2

                For j As Integer = 1 To 4

                    If j = 1 Then
                        A = data.JambLeftXA(st)
                        b = data.JambLeftYA(st)
                        C = data.JambLeftXB(st)
                        D = data.JambLeftYB(st)
                    End If

                    If j = 2 Then
                        A = data.JambLeftXC(st)
                        b = data.JambLeftYC(st)
                        C = data.JambLeftXD(st)
                        D = data.JambLeftYD(st)
                    End If

                    If j = 3 Then
                        A = data.JambLeftXA(st)
                        b = data.JambLeftYA(st)
                        C = data.JambLeftXC(st)
                        D = data.JambLeftYC(st)
                    End If

                    If j = 4 Then
                        A = data.JambLeftXB(st)
                        b = data.JambLeftYB(st)
                        C = data.JambLeftXD(st)
                        D = data.JambLeftYD(st)
                    End If
                    DrawJambLine("JambLeft", MyFile, A, b, C, D)
                Next j
            Next st

            For st = 0 To data.NumberOfOpening - 2

                For j As Integer = 1 To 4

                    If j = 1 Then
                        A = data.JambRightXA(st)
                        b = data.JambRightYA(st)
                        C = data.JambRightXB(st)
                        D = data.JambRightYB(st)
                    End If

                    If j = 2 Then
                        A = data.JambRightXC(st)
                        b = data.JambRightYC(st)
                        C = data.JambRightXD(st)
                        D = data.JambRightYD(st)
                    End If

                    If j = 3 Then
                        A = data.JambRightXA(st)
                        b = data.JambRightYA(st)
                        C = data.JambRightXC(st)
                        D = data.JambRightYC(st)
                    End If

                    If j = 4 Then
                        A = data.JambRightXB(st)
                        b = data.JambRightYB(st)
                        C = data.JambRightXD(st)
                        D = data.JambRightYD(st)
                    End If
                    DrawJambLine("JambRight", MyFile, A, b, C, D)
                Next j
                data.VERTICALMEMBER(offset + st) = "JambRight"
            Next st

            For st = 1 To 2 * (data.endOfPost)
                If data.EndPostStYB(st) < 1 Or data.EndPostStYD(st) < 1 Then
                    Continue For
                End If
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.EndPostStXA(st)
                        b = data.EndPostStYA(st)
                        C = data.EndPostStXB(st)
                        D = data.EndPostStYB(st)
                    End If
                    If j = 2 Then
                        A = data.EndPostStXC(st)
                        b = data.EndPostStYC(st)
                        C = data.EndPostStXD(st)
                        D = data.EndPostStYD(st)
                    End If
                    If j = 3 Then
                        A = data.EndPostStXA(st)
                        b = data.EndPostStYA(st)
                        C = data.EndPostStXC(st)
                        D = data.EndPostStYC(st)
                    End If
                    If j = 4 Then
                        A = data.EndPostStXB(st)
                        b = data.EndPostStYB(st)
                        C = data.EndPostStXD(st)
                        D = data.EndPostStYD(st)
                    End If
                    If st <= data.endOfPost Then
                        DrawEndPostLine("EndPostleft", MyFile, A, b, C, D)
                    Else
                        DrawEndPostLine("EndPostRight", MyFile, A, b, C, D)
                    End If
                Next j
                data.VERTICALMEMBER(offset + st) = "JambRight"
            Next st

            For st = 1 To data.NumberOfInteriorPost
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.InterMetdiatPostAX(st)
                        b = data.InterMetdiatPostAY(st)
                        C = data.InterMetdiatPostBX(st)
                        D = data.InterMetdiatPostBY(st)
                    End If
                    If j = 2 Then
                        A = data.InterMetdiatPostCX(st)
                        b = data.InterMetdiatPostCY(st)
                        C = data.InterMetdiatPostDX(st)
                        D = data.InterMetdiatPostDY(st)
                    End If
                    If j = 3 Then
                        A = data.InterMetdiatPostAX(st)
                        b = data.InterMetdiatPostAY(st)
                        C = data.InterMetdiatPostCX(st)
                        D = data.InterMetdiatPostCY(st)
                    End If
                    If j = 4 Then
                        A = data.InterMetdiatPostBX(st)
                        b = data.InterMetdiatPostBY(st)
                        C = data.InterMetdiatPostDX(st)
                        D = data.InterMetdiatPostDY(st)
                    End If
                    lineColor = 9
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 1 To data.NumberOfOpening - 1

                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.OpeningAX(st)
                        b = data.OpeningAY(st)
                        C = data.OpeningBX(st)
                        D = data.OpeningBY(st)
                    End If
                    If j = 2 Then
                        A = data.OpeningCX(st)
                        b = data.OpeningCY(st)
                        C = data.OpeningDX(st)
                        D = data.OpeningDY(st)
                    End If
                    If j = 3 Then
                        A = data.OpeningAX(st)
                        b = data.OpeningAY(st)
                        C = data.OpeningCX(st)
                        D = data.OpeningCY(st)
                    End If
                    If j = 4 Then
                        A = data.OpeningBX(st)
                        b = data.OpeningBY(st)
                        C = data.OpeningDX(st)
                        D = data.OpeningDY(st)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 1 To data.NumberOfOpening
                If data.TopTrackAYO(st) = 0 Then
                    Continue For
                End If
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.TopTrackAXO(st)
                        b = data.TopTrackAYO(st)
                        C = data.TopTrackBXO(st)
                        D = data.TopTrackBYO(st)
                    End If
                    If j = 2 Then
                        A = data.TopTrackCXO(st)
                        b = data.TopTrackCYO(st)
                        C = data.TopTrackDXO(st)
                        D = data.TopTrackDYO(st)
                    End If
                    If j = 3 Then
                        A = data.TopTrackAXO(st)
                        b = data.TopTrackAYO(st)
                        C = data.TopTrackCXO(st)
                        D = data.TopTrackCYO(st)
                    End If
                    If j = 4 Then
                        A = data.TopTrackBXO(st)
                        b = data.TopTrackBYO(st)
                        C = data.TopTrackDXO(st)
                        D = data.TopTrackDYO(st)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 1 To data.NumberOfOpening - 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.BottomTrackAXO(st)
                        b = data.BottomTrackAYO(st)
                        C = data.BottomTrackBXO(st)
                        D = data.BottomTrackBYO(st)
                    End If
                    If j = 2 Then
                        A = data.BottomTrackCXO(st)
                        b = data.BottomTrackCYO(st)
                        C = data.BottomTrackDXO(st)
                        D = data.BottomTrackDYO(st)
                    End If
                    If j = 3 Then
                        A = data.BottomTrackAXO(st)
                        b = data.BottomTrackAYO(st)
                        C = data.BottomTrackCXO(st)
                        D = data.BottomTrackCYO(st)
                    End If
                    If j = 4 Then
                        A = data.BottomTrackBXO(st)
                        b = data.BottomTrackBYO(st)
                        C = data.BottomTrackDXO(st)
                        D = data.BottomTrackDYO(st)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 0 To data.NumberOfSlope - 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = topbktrackData.AX(st)
                        b = topbktrackData.AY(st)
                        C = topbktrackData.BX(st)
                        D = topbktrackData.BY(st)
                    End If
                    If j = 2 Then
                        A = topbktrackData.CX(st)
                        b = topbktrackData.CY(st)
                        C = topbktrackData.DX(st)
                        D = topbktrackData.DY(st)
                    End If
                    If j = 3 Then
                        A = topbktrackData.AX(st)
                        b = topbktrackData.AY(st)
                        C = topbktrackData.CX(st)
                        D = topbktrackData.CY(st)
                    End If
                    If j = 4 Then
                        A = topbktrackData.BX(st)
                        b = topbktrackData.BY(st)
                        C = topbktrackData.DX(st)
                        D = topbktrackData.DY(st)
                    End If
                    lineColor = 3
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 1 To 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = topbktrackData.BottomTrackAX
                        b = topbktrackData.BottomTrackAY
                        C = topbktrackData.BottomTrackBX
                        D = topbktrackData.BottomTrackBY
                    End If
                    If j = 2 Then
                        A = topbktrackData.BottomTrackCX
                        b = topbktrackData.BottomTrackCY
                        C = topbktrackData.BottomTrackDX
                        D = topbktrackData.BottomTrackDY
                    End If
                    If j = 3 Then
                        A = topbktrackData.BottomTrackAX
                        b = topbktrackData.BottomTrackAY
                        C = topbktrackData.BottomTrackCX
                        D = topbktrackData.BottomTrackCY
                    End If
                    If j = 4 Then
                        A = topbktrackData.BottomTrackBX
                        b = topbktrackData.BottomTrackBY
                        C = topbktrackData.BottomTrackDX
                        D = topbktrackData.BottomTrackDY
                    End If
                    lineColor = 5
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next
            Next

            For st = 0 To data.NumberOfOpening - 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.ShearPlateLeftAX(st)
                        b = data.ShearPlateLeftAY(st)
                        C = data.ShearPlateLeftBX(st)
                        D = data.ShearPlateLeftBY(st)
                    End If
                    If j = 2 Then
                        A = data.ShearPlateLeftCX(st)
                        b = data.ShearPlateLeftCY(st)
                        C = data.ShearPlateLeftDX(st)
                        D = data.ShearPlateLeftDY(st)
                    End If
                    If j = 3 Then
                        A = data.ShearPlateLeftAX(st)
                        b = data.ShearPlateLeftAY(st)
                        C = data.ShearPlateLeftCX(st)
                        D = data.ShearPlateLeftCY(st)
                    End If
                    If j = 4 Then
                        A = data.ShearPlateLeftBX(st)
                        b = data.ShearPlateLeftBY(st)
                        C = data.ShearPlateLeftDX(st)
                        D = data.ShearPlateLeftDY(st)
                    End If
                    lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 0 To data.NumberOfOpening - 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.ShearPlateRightAX(st)
                        b = data.ShearPlateRightAY(st)
                        C = data.ShearPlateRightBX(st)
                        D = data.ShearPlateRightBY(st)
                    End If
                    If j = 2 Then
                        A = data.ShearPlateRightCX(st)
                        b = data.ShearPlateRightCY(st)
                        C = data.ShearPlateRightDX(st)
                        D = data.ShearPlateRightDY(st)
                    End If
                    If j = 3 Then
                        A = data.ShearPlateRightAX(st)
                        b = data.ShearPlateRightAY(st)
                        C = data.ShearPlateRightCX(st)
                        D = data.ShearPlateRightCY(st)
                    End If
                    If j = 4 Then
                        A = data.ShearPlateRightBX(st)
                        b = data.ShearPlateRightBY(st)
                        C = data.ShearPlateRightDX(st)
                        D = data.ShearPlateRightDY(st)
                    End If
                    lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st


            For st = 0 To hblockingNumber
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.HorizontalBlockingAX(st)
                        b = data.HorizontalBlockingAY(st)
                        C = data.HorizontalBlockingBX(st)
                        D = data.HorizontalBlockingBY(st)
                    End If
                    If j = 2 Then
                        A = data.HorizontalBlockingCX(st)
                        b = data.HorizontalBlockingCY(st)
                        C = data.HorizontalBlockingDX(st)
                        D = data.HorizontalBlockingDY(st)
                    End If
                    If j = 3 Then
                        A = data.HorizontalBlockingAX(st)
                        b = data.HorizontalBlockingAY(st)
                        C = data.HorizontalBlockingCX(st)
                        D = data.HorizontalBlockingCY(st)
                    End If
                    If j = 4 Then
                        A = data.HorizontalBlockingBX(st)
                        b = data.HorizontalBlockingBY(st)
                        C = data.HorizontalBlockingDX(st)
                        D = data.HorizontalBlockingDY(st)
                    End If
                    lineColor = 6
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 1 To numberofStrips
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.HorizontalStrapHeightAx(st)
                        b = data.HorizontalStrapHeightAy(st)
                        C = data.HorizontalStrapHeightBx(st)
                        D = data.HorizontalStrapHeightBy(st)
                    End If
                    If j = 2 Then
                        A = data.HorizontalStrapHeightCx(st)
                        b = data.HorizontalStrapHeightCy(st)
                        C = data.HorizontalStrapHeightDx(st)
                        D = data.HorizontalStrapHeightDy(st)
                    End If
                    If j = 3 Then
                        A = data.HorizontalStrapHeightAx(st)
                        b = data.HorizontalStrapHeightAy(st)
                        C = data.HorizontalStrapHeightCx(st)
                        D = data.HorizontalStrapHeightCy(st)
                    End If
                    If j = 4 Then
                        A = data.HorizontalStrapHeightBx(st)
                        b = data.HorizontalStrapHeightBy(st)
                        C = data.HorizontalStrapHeightDx(st)
                        D = data.HorizontalStrapHeightDy(st)
                    End If
                    lineColor = 8
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

            For st = 0 To 2 * (data.NumberofIntersectionWall - 1) - 1
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.IntersectionWallAX(st)
                        b = data.IntersectionWallAY(st)
                        C = data.IntersectionWallBX(st)
                        D = data.IntersectionWallBy(st)
                    End If
                    If j = 2 Then
                        A = data.IntersectionWallcX(st)
                        b = data.IntersectionWallcy(st)
                        C = data.IntersectionWalldX(st)
                        D = data.IntersectionWalldy(st)
                    End If
                    If j = 3 Then
                        A = data.IntersectionWallAX(st)
                        b = data.IntersectionWallAY(st)
                        C = data.IntersectionWallcX(st)
                        D = data.IntersectionWallcy(st)
                    End If
                    If j = 4 Then
                        A = data.IntersectionWallBX(st)
                        b = data.IntersectionWallBy(st)
                        C = data.IntersectionWalldX(st)
                        D = data.IntersectionWalldy(st)
                    End If
                    lineColor = 4
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
            Next st

EndCase:


            MyFile.WriteLine("ENDSEC")
            MyFile.WriteLine(0)
            MyFile.WriteLine("EOF")
            MyFile.Close()


            Dim machine = New MachineFabricationExports.FabricationExport()
            machine.Export(dxfFilePath.Split(".")(0) + "-machine-febrication.xlsx", dxfFilePath)
        Catch ex As Exception
            Return
        End Try


    End Sub

    Private Sub DRAWLINE(MyFile As StreamWriter, lineColor As Double, A As Double, b As Double, C As Double, D As Double)
        ' DRAW A LINE
        MyFile.WriteLine("LINE")
        MyFile.WriteLine(8)
        MyFile.WriteLine(2)
        MyFile.WriteLine(62)
        MyFile.WriteLine(lineColor)
        MyFile.WriteLine(10)
        MyFile.WriteLine(A) ''x1
        MyFile.WriteLine(20)
        MyFile.WriteLine(b) ''y1
        MyFile.WriteLine(11)
        MyFile.WriteLine(C) ''x2
        MyFile.WriteLine(21)
        MyFile.WriteLine(D) ''x3
        MyFile.WriteLine(31)
        MyFile.WriteLine(0)
        MyFile.WriteLine(0)
    End Sub

    Private Function CheckData() As Boolean
        If MainForm.inputData.studsFlange = 0 Or MainForm.inputData.studsSpacing = 0 Then
            Return False
        End If
        Return True
    End Function

    Dim verticalElement_temp = 0
    Private Sub VerticalElementsAssemble(heightWall As Double)
        verticalElement_temp = VerticalElement
        Dim data = MainForm.inputData
        For k = 0 To 1000 + VerticalElement + n
            data.AXX(k) = 0
            data.AYY(k) = 0
            data.BXX(k) = 0
            data.BYY(k) = 0
            data.CXX(k) = 0
            data.CYY(k) = 0
            data.DXX(k) = 0
            data.DYY(k) = 0
        Next

        Dim pos = 0
        For position = 0 To VerticalElement + n
            If data.StudsPointAX(position) = 0 And data.StudsPointBY(position) = 0 Or data.StudsPointCX(position) = 0 Or data.AXX(position) > data.wallLength Or data.StudsPointAY(position) > heightWall Or data.StudsPointBY(position) < heightWall Then
                Continue For
            End If

            data.AXX(pos) = data.StudsPointAX(position)
            data.BXX(pos) = data.StudsPointBX(position)
            data.CXX(pos) = data.StudsPointCX(position)
            data.DXX(pos) = data.StudsPointDX(position)
            data.AYY(pos) = data.StudsPointAY(position)
            data.BYY(pos) = data.StudsPointBY(position)
            data.CYY(pos) = data.StudsPointCY(position)
            data.DYY(pos) = data.StudsPointDY(position)

            pos += 1
        Next

        VerticalElement = pos + 1
        pos = 0
        For position = 0 To (2 * data.NumberOfOpening)

            If data.JambAX(position) = 0 And data.JambBY(position) = 0 Or data.JambCX(position) = 0 Or data.JambAX(position) > data.wallLength Or data.JambBY(position) < heightWall Or data.JambBY(position) > heightWall And heightWall > data.wallHeight Then
                Continue For
            End If

            data.AXX(VerticalElement + pos) = data.JambAX(position)
            data.BXX(VerticalElement + pos) = data.JambBX(position)
            data.CXX(VerticalElement + pos) = data.JambCX(position)
            data.DXX(VerticalElement + pos) = data.JambDX(position)
            data.AYY(VerticalElement + pos) = data.JambAY(position)
            data.BYY(VerticalElement + pos) = data.JambBY(position)
            data.CYY(VerticalElement + pos) = data.JambCY(position)
            data.DYY(VerticalElement + pos) = data.JambDY(position)
            pos += 1
        Next


        VerticalElement = VerticalElement + pos + 1
        If data.endPostBox Then
            For position = 1 To 2 * data.endOfPost
                If data.EndPostStYB(position) <= heightWall Then
                    Continue For
                End If
                data.AXX(VerticalElement + position) = data.EndPostStXA(position)
                data.BXX(VerticalElement + position) = data.EndPostStXB(position)
                data.CXX(VerticalElement + position) = data.EndPostStXC(position)
                data.DXX(VerticalElement + position) = data.EndPostStXD(position)
                data.AYY(VerticalElement + position) = data.EndPostStYA(position)
                data.BYY(VerticalElement + position) = data.EndPostStYB(position)
                data.CYY(VerticalElement + position) = data.EndPostStYC(position)
                data.DYY(VerticalElement + position) = data.EndPostStYD(position)
                data.EndPostLength(position) = data.EndPostStYD(position) - data.EndPostStYC(position)
            Next

            VerticalElement = 2 * data.endOfPost + VerticalElement + 1

        End If
        pos = 0
        For position = 1 To data.NumberOfInteriorPost
            'skip
            If data.InterMetdiatPostBY(position) < heightWall Or data.InterMetdiatPostAY(position) > heightWall Then
                Continue For
            End If

            data.AXX(VerticalElement + pos) = data.InterMetdiatPostAX(position)
            data.BXX(VerticalElement + pos) = data.InterMetdiatPostBX(position)
            data.CXX(VerticalElement + pos) = data.InterMetdiatPostCX(position)
            data.DXX(VerticalElement + pos) = data.InterMetdiatPostDX(position)
            data.AYY(VerticalElement + pos) = data.InterMetdiatPostAY(position)
            data.BYY(VerticalElement + pos) = data.InterMetdiatPostBY(position)
            data.CYY(VerticalElement + pos) = data.InterMetdiatPostCY(position)
            data.DYY(VerticalElement + pos) = data.InterMetdiatPostDY(position)
            data.InterMediatPostLength(pos) = data.InterMetdiatPostDY(position) - data.InterMetdiatPostCY(position)

            pos += 1
        Next

        VerticalElement = data.NumberOfInteriorPost + VerticalElement + pos + 1

        For position = 0 To 2 * (data.NumberofIntersectionWall - 1)
            If data.IntersectionWallBy(position) <= heightWall Then
                Continue For
            End If

            data.AXX(VerticalElement + pos) = data.IntersectionWallAX(position)
            data.BXX(VerticalElement + pos) = data.IntersectionWallBX(position)
            data.CXX(VerticalElement + pos) = data.IntersectionWallcX(position)
            data.DXX(VerticalElement + pos) = data.IntersectionWalldX(position)
            data.AYY(VerticalElement + pos) = data.IntersectionWallAY(position)
            data.BYY(VerticalElement + pos) = data.IntersectionWallBy(position)
            data.CYY(VerticalElement + pos) = data.IntersectionWallcy(position)
            data.DYY(VerticalElement + pos) = data.IntersectionWalldy(position)
            pos += 1
        Next

        VerticalElement += pos
        For i = 0 To VerticalElement
            For k = i + 1 To VerticalElement
                If (data.AXX(i) > data.AXX(k)) Then
                    Dim AX = data.AXX(i)
                    Dim AY = data.AYY(i)
                    Dim BX = data.BXX(i)
                    Dim BY = data.BYY(i)
                    Dim CX = data.CXX(i)
                    Dim CY = data.CYY(i)
                    Dim DX = data.DXX(i)
                    Dim DY = data.DYY(i)

                    data.AXX(i) = data.AXX(k)
                    data.AYY(i) = data.AYY(k)
                    data.BXX(i) = data.BXX(k)
                    data.BYY(i) = data.BYY(k)
                    data.CXX(i) = data.CXX(k)
                    data.CYY(i) = data.CYY(k)
                    data.DXX(i) = data.DXX(k)
                    data.DYY(i) = data.DYY(k)

                    data.AXX(k) = AX
                    data.AYY(k) = AY
                    data.BXX(k) = BX
                    data.BYY(k) = BY
                    data.CXX(k) = CX
                    data.CYY(k) = CY
                    data.DXX(k) = DX
                    data.DYY(k) = DY
                End If
            Next

        Next
    End Sub
    Private Sub CheckNextMember(V As Integer, i As Integer)


        Dim data = MainForm.inputData

        For k = V + 1 To VerticalElement
            If data.BlockingHeight(i) >= data.AYY(k) And data.BlockingHeight(i) <= data.BYY(k) Then
                TotalNumberOfElement = TotalNumberOfElement + 1
                If data.WidthofHorizontalBlocing(i) < 2.5 Then data.AXX(TotalNumberOfElement) = data.CXX(V)
                If data.WidthofHorizontalBlocing(i) > 2.5 Then data.AXX(TotalNumberOfElement) = data.CXX(V) - 0.5 * data.StudsWidth
                data.AYY(TotalNumberOfElement) = data.BlockingHeight(i)
                If data.WidthofHorizontalBlocing(i) < 2.5 Then data.BXX(TotalNumberOfElement) = data.DXX(V)
                If data.WidthofHorizontalBlocing(i) > 2.5 Then data.BXX(TotalNumberOfElement) = data.DXX(V) - 0.5 * data.StudsWidth
                data.BYY(TotalNumberOfElement) = data.BlockingHeight(i) + data.WidthofHorizontalBlocing(i)
                If data.WidthofHorizontalBlocing(i) < 2.5 Then data.CXX(TotalNumberOfElement) = data.AXX(k)
                If data.WidthofHorizontalBlocing(i) > 2.5 Then data.CXX(TotalNumberOfElement) = data.AXX(k) + 0.5 * data.StudsWidth
                data.CYY(TotalNumberOfElement) = data.BlockingHeight(i)


                If data.WidthofHorizontalBlocing(i) < 2.5 Then data.DXX(TotalNumberOfElement) = data.AXX(k)
                If data.WidthofHorizontalBlocing(i) > 2.5 Then data.DXX(TotalNumberOfElement) = data.AXX(k) + 0.5 * data.StudsWidth
                data.DYY(TotalNumberOfElement) = data.BlockingHeight(i) + data.WidthofHorizontalBlocing(i)
                If data.WidthofHorizontalBlocing(i) < 2.5 Then data.BlockingName$(TotalNumberOfElement) = "Blocking H."
                If data.WidthofHorizontalBlocing(i) > 2.5 Then data.BlockingName$(TotalNumberOfElement) = "Blocking V."
                Dim ASTIFNERX = data.AXX(TotalNumberOfElement)
                Dim ASTIFNERY = data.AYY(TotalNumberOfElement)
                Dim BSTIFNERX = data.BXX(TotalNumberOfElement)
                Dim BSTIFNERY = data.BYY(TotalNumberOfElement)
                Dim CSTIFNERX = data.CXX(TotalNumberOfElement)
                Dim CSTIFNERY = data.CYY(TotalNumberOfElement)
                Dim DSTIFNERX = data.DXX(TotalNumberOfElement)
                Dim DSTIFNERY = data.DYY(TotalNumberOfElement)
            End If
        Next k
        Return
    End Sub


    Private Sub HorizontalBlocking()
        Dim data = MainForm.inputData
        TotalNumberOfElement = VerticalElement
        ' End reading all vertical member
        ' check all extra blocking and stifenner
        Dim TotalWeight = 0
        'reading all vertical member
        Dim numberofnode = VerticalElement

        Dim NumberOfLineHorizontalBlocking = data.NumberOfBlockingElement 'Cells(34, 6)
        For i = 1 To NumberOfLineHorizontalBlocking
            For V = 0 To VerticalElement - 1
                If data.BlockingHeight(i) >= data.AYY(V) And data.BlockingHeight(i) <= data.BYY(V) Then
                    'GoSub CheckNextMember 10
                    CheckNextMember(V, i)
                End If
            Next V
        Next i

        For T = 1 To data.NumberOfOpening
            If data.AXX(TotalNumberOfElement) <= data.LeftXBoundary(T) And data.CXX(TotalNumberOfElement) >= data.RightXboundary(T) Then
                'GoSub CheckBlockingHeight
                CheckBlockingHeight(T)
            End If
        Next T
    End Sub

    Private Sub CheckBlockingHeight(T As Integer)
        Dim data = MainForm.inputData

        If data.AYY(TotalNumberOfElement) > data.SillOpening(T) And data.AYY(TotalNumberOfElement) <= data.SillOpening(T) + data.HeightOpening(T) + data.StudsWidth Then
            data.AXX(TotalNumberOfElement) = 0
            data.AYY(TotalNumberOfElement) = 0
            data.BXX(TotalNumberOfElement) = 0
            data.BYY(TotalNumberOfElement) = 0
            data.CXX(TotalNumberOfElement) = 0
            data.CYY(TotalNumberOfElement) = 0
            data.DXX(TotalNumberOfElement) = 0
            data.DYY(TotalNumberOfElement) = 0
        End If
        Return
    End Sub


    Public Function DimensionScaling(maxYY As Double, maxXX As Double) As ScalDimension
        Dim RatioX = 0
        Dim RatioY = 0
        Dim ScaleRatio = maxYY / maxXX
        Dim Lmax = 0, LmaxY = 0

        Dim scalee As Integer
        If ScaleRatio <= 0.66668 Then

            If maxXX <= 288 Then
                Lmax = 288
                LmaxY = 192
                scalee = 24 / 12
            End If

            If maxXX > 288 And maxXX < 576 Then
                Lmax = 576
                LmaxY = 384
                scalee = 48 / 12
            End If

            If maxXX > 576 And maxXX < 864 Then
                Lmax = 864 : LmaxY = 576
                scalee = 72 / 12
            End If

            If maxXX > 864 And maxXX < 1152 Then
                Lmax = 1152 : LmaxY = 768
                scalee = 96 / 12
            End If

            If maxXX > 1152 And maxXX < 1440 Then
                Lmax = 1440 : LmaxY = 960
                scalee = 120 / 12
            End If

            If maxXX > 1440 And maxXX < 1728 Then
                Lmax = 1728 : LmaxY = 1152
                scalee = 144 / 12
            End If

            If maxXX > 1728 And maxXX < 2016 Then
                Lmax = 2016 : LmaxY = 1344
                scalee = 168 / 12
            End If

            If maxXX > 2016 And maxXX < 2304 Then
                Lmax = 2304 : LmaxY = 1536
                scalee = 192 / 12
            End If

            If maxXX > 2304 And maxXX < 2592 Then
                Lmax = 2592 : LmaxY = 1728
                scalee = 216 / 12
            End If
            RatioX = 848.19 / Lmax
            RatioY = 621.456 / LmaxY
        End If

        If ScaleRatio > 0.66668 Then
            If maxYY <= 192 Then
                LmaxY = 192
                Lmax = 288
                scalee = 24 / 12
            End If

            If maxYY > 192 And maxYY < 384 Then
                LmaxY = 384
                Lmax = 576
                scalee = 48 / 12
            End If

            If maxYY > 384 And maxYY < 576 Then
                LmaxY = 576
                Lmax = 864
                scalee = 72 / 12
            End If

            If maxYY > 576 And maxYY < 768 Then
                LmaxY = 768 : Lmax = 1152
                scalee = 96 / 12
            End If

            If maxYY > 768 And maxYY < 960 Then
                LmaxY = 960
                Lmax = 1440
                scalee = 120 / 12
            End If

            If maxYY > 960 And maxYY < 1152 Then
                LmaxY = 1152 : Lmax = 1728
                scalee = 144 / 12
            End If

            If maxYY > 1152 And maxYY < 1344 Then
                LmaxY = 1344 : Lmax = 2016
                scalee = 168 / 12
            End If

            If maxYY > 1344 And maxYY < 1536 Then
                LmaxY = 1536 : Lmax = 2304
                scalee = 192 / 12
            End If

            If maxYY > 1536 And maxYY < 1728 Then
                LmaxY = 1728
                Lmax = 2592
                scalee = 216 / 12
            End If

            RatioX = 848.19 / Lmax
            RatioY = 621.456 / LmaxY

        End If

        Dim ScaleDrawings As Integer = 1

        RatioX = RatioX * ScaleDrawings
        RatioY = RatioY * ScaleDrawings

        Me.uiWindow.scaleTxtLbl.Content = scalee / ScaleDrawings

        Dim OffsetX = 50
        Dim OffsetY = 704 - 50
        Return New ScalDimension() With {
        .OffsetX = OffsetX,
        .OffsetY = OffsetY,
        .RatioX = RatioX,
        .RatioY = RatioY,
        .units = scalee / ScaleDrawings
        }

    End Function
    Public Function CreateDFXScriptFile() As String
        Dim data = MainForm.inputData
        Dim fsokamel, MyFilekamel

        fsokamel = CreateObject("Scripting.FileSystemObject")
        MyFilekamel = fsokamel.CreateTextFile((fileDxf.dxfFilePath + "Script.scr"), True)
        Dim counter = 1

        Dim skiped = 0
        AssignNumericLableToElements(0, 0, 0, 0, False)
        For st = 0 To verticalElement_temp
            If data.BYY(st) = 0 Then
                skiped += 1
                Continue For
            End If
            Dim xa = data.AXX(st)
            Dim ya = data.AYY(st)

            Dim xb = data.BXX(st)
            Dim yb = data.BYY(st)

            Dim xc = data.CXX(st)
            Dim yc = data.CYY(st)

            Dim xd = data.DXX(st)
            Dim yd = data.DYY(st)
            Dim Xcenter = (xa + xc) / 2
            Dim Lengthxx = Math.Abs(xc - xa)
            Dim YYY

            If data.VERTICALMEMBER(st) = "Studs" Then YYY = yb
            If data.VERTICALMEMBER(st) = "Top Track" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Bottom Track" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Opening Bottom Track" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Opening Top Track" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Jambs" Then YYY = yb
            If data.VERTICALMEMBER(st) = "H.Blocking" Then YYY = yd
            If data.VERTICALMEMBER(st) = "V.Blocking" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Horizontal Strap" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Header" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Shear Plate Left" Then YYY = yd
            If data.VERTICALMEMBER(st) = "Shear Plate Right" Then YYY = yd
            If data.VERTICALMEMBER(st) = "End Post" Then YYY = yb
            If data.VERTICALMEMBER(st) = "Intersection Wall" Then YYY = yb
            '----------------------------------------

            Dim Ycenter = (ya + YYY) / 2
            Dim SectionWidth1

            If data.VERTICALMEMBER(st) = "Studs" Then SectionWidth1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Jambs" Then SectionWidth1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "JambRight" Then SectionWidth1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "End Post" Then SectionWidth1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Interior Post" Then SectionWidth1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Header" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Top Track" Then SectionWidth1 = Math.Abs(Math.Sqrt(((xc - xa) * (xc - xa) + (yc - ya) * (yc - ya))))
            If data.VERTICALMEMBER(st) = "Bottom Track" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Opening Bottom Track" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Opening Top Track" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Horizontal Strap" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "H.Blocking" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "V.Blocking" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Shear Plate Left" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Shear Plate Right" Then SectionWidth1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Intersection Wall" Then SectionWidth1 = Math.Abs(xc - xa)

            Dim SectionHeight1

            If data.VERTICALMEMBER(st) = "Studs" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Jambs" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "JambRight" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "End Post" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Interior Post" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Header" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Top Track" Then SectionHeight1 = Math.Abs(yb - ya)
            If data.VERTICALMEMBER(st) = "Bottom Track" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Opening Bottom Track" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Opening Top Track" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Horizontal Strap" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "H.Blocking" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "V.Blocking" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Shear Plate Left" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Shear Plate Right" Then SectionHeight1 = Math.Abs(xc - xa)
            If data.VERTICALMEMBER(st) = "Intersection Wall" Then SectionHeight1 = Math.Abs(yb - ya)

            MyFilekamel.WriteLine("TEXT " & Xcenter & "," & Ycenter & " " & 2 & " " & "0" & " " & counter)
            Dim Xline = data.wallLength + 20
            Dim Yline = data.wallHeight - 4 * counter

            MyFilekamel.WriteLine("TEXT " & Xline & "," & Yline & " " & 2 & " " & "0" & " " & counter & " " & data.VERTICALMEMBER(st) & " " & SectionHeight1 & " , Width  " & CType(Double.Parse(SectionWidth1).ToString("0.00"), Double) & " Inch ")
            counter = counter + 1
            SectionWidth1 = 0

        Next st
        MyFilekamel.Close

    End Function


    Public Sub CreateShopDrawingElements()
        Try
            Dim data = MainForm.inputData

            data.shopDrawingElements = New ShopDrawingElements() With {
            .elements = New List(Of Element)
            }

            Dim drCoord = data.shopDrawingElements

            For st = 0 To totalNumberOfStuds
                data.AXX(st) = data.StudsPointAX(st)
                data.AYY(st) = data.StudsPointAY(st)
                data.BXX(st) = data.StudsPointBX(st)
                data.BYY(st) = data.StudsPointBY(st)
                data.CXX(st) = data.StudsPointCX(st)
                data.CYY(st) = data.StudsPointCY(st)
                data.DXX(st) = data.StudsPointDX(st)
                data.DYY(st) = data.StudsPointDY(st)
                data.VERTICALMEMBER(st) = "Studs"
            Next st



            '++++++++++++++++CODE TO CREAT A DXF FILE AS SHOWN BELOW +++++++++++++++
            'mmmmmmmmmmmmmmmmmmmmmmmmmm

            Dim path As String = "c:\temp\WALL1.dxf"
            If System.IO.Directory.Exists("c:\temp\") = False Then
                System.IO.Directory.CreateDirectory("c:\temp\")
            End If

            ' Create or overwrite the file.

            Dim MyFile = File.CreateText(path)


#Region "Comment"
            ' Add text to the file.
            'Dim info As Byte() = New UTF8Encoding(True).GetBytes("This is some text in the file.")
            'fs.Write(info, 0, info.Length)
            'fs.Close()

            'fso = CreateObject("Scripting.FileSystemObject")

            'MyFile = fso..CreateTextFile("c:\casa\WALL1.dxf", True)
#End Region

            MyFile.WriteLine(0)
            MyFile.WriteLine("SECTION")
            MyFile.WriteLine(2)
            MyFile.WriteLine("ENTITIES")
            MyFile.WriteLine(0)


            Dim A As Decimal = 0, b As Decimal = 0, C As Decimal = 0, D As Decimal = 0
            Dim lineColor = 1
            For st As Integer = 0 To totalNumberOfStuds
                If data.StudsPointBY(st) < 1 Or data.StudsPointDY(st) < 1 Then
                    Continue For
                End If

                Dim element = New Element()
                For j As Integer = 1 To 4

                    If j = 1 Then
                        A = data.StudsPointAX(st)
                        b = data.StudsPointAY(st)
                        C = data.StudsPointBX(st)
                        D = data.StudsPointBY(st)

                        element.LineA = CreateLine(A, b, C, D)

                    End If

                    If j = 2 Then
                        A = data.StudsPointCX(st)
                        b = data.StudsPointCY(st)
                        C = data.StudsPointDX(st)
                        D = data.StudsPointDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If

                    If j = 3 Then
                        A = data.StudsPointAX(st)
                        b = data.StudsPointAY(st)
                        C = data.StudsPointCX(st)
                        D = data.StudsPointCY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If

                    If j = 4 Then
                        A = data.StudsPointBX(st)
                        b = data.StudsPointBY(st)
                        C = data.StudsPointDX(st)
                        D = data.StudsPointDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If

                    If data.VERTICALMEMBER(st) = "Studs" Then
                        element.elementType = "Studs"
                        lineColor = 1
                    Else
                        lineColor = 9
                    End If

                    If data.VERTICALMEMBER(st) = "EndPostleft" Then lineColor = 2
                    If data.VERTICALMEMBER(st) = "EndpostRight" Then lineColor = 0
                    If data.VERTICALMEMBER(st) = "TopTrack" Then lineColor = 3
                    If data.VERTICALMEMBER(st) = "JambLeft" Then lineColor = 4
                    If data.VERTICALMEMBER(st) = "JambRight" Then lineColor = 4
                    If data.VERTICALMEMBER(st) = "BottomTrack" Then lineColor = 5
                    If data.VERTICALMEMBER(st) = "Blocking H." Then lineColor = 6
                    If data.VERTICALMEMBER(st) = "Blocking V." Then lineColor = 7
                    If data.VERTICALMEMBER(st) = "Hori.Strap" Then lineColor = 8
                    If data.VERTICALMEMBER(st) = "Headerx" Then lineColor = 10
                    If data.VERTICALMEMBER(st) = "Header" Then lineColor = 10
                    If data.VERTICALMEMBER(st) = "ShearPlateLeft" Then lineColor = 11
                    If data.VERTICALMEMBER(st) = "ShearPlateRight" Then lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)

                Next j
                'ELEMENTS
                drCoord.elements.Add(element)
            Next st

            Dim offset = VerticalElement + n - 1 + 1
            Dim index = 0
            For st = 0 To data.NumberOfOpening - 2
                Dim element = New Element()
                For j As Integer = 1 To 4

                    If j = 1 Then
                        A = data.JambLeftXA(st)
                        b = data.JambLeftYA(st)
                        C = data.JambLeftXB(st)
                        D = data.JambLeftYB(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If

                    If j = 2 Then
                        A = data.JambLeftXC(st)
                        b = data.JambLeftYC(st)
                        C = data.JambLeftXD(st)
                        D = data.JambLeftYD(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If

                    If j = 3 Then
                        A = data.JambLeftXA(st)
                        b = data.JambLeftYA(st)
                        C = data.JambLeftXC(st)
                        D = data.JambLeftYC(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If

                    If j = 4 Then
                        A = data.JambLeftXB(st)
                        b = data.JambLeftYB(st)
                        C = data.JambLeftXD(st)
                        D = data.JambLeftYD(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If

                    DrawJambLine("JambLeft", MyFile, A, b, C, D)
                Next j

                element.elementType = "JambLeft"
                drCoord.elements.Add(element)
            Next st

            For st = 0 To data.NumberOfOpening - 2
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.JambRightXA(st)
                        b = data.JambRightYA(st)
                        C = data.JambRightXB(st)
                        D = data.JambRightYB(st)
                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.JambRightXC(st)
                        b = data.JambRightYC(st)
                        C = data.JambRightXD(st)
                        D = data.JambRightYD(st)
                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.JambRightXA(st)
                        b = data.JambRightYA(st)
                        C = data.JambRightXC(st)
                        D = data.JambRightYC(st)
                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.JambRightXB(st)
                        b = data.JambRightYB(st)
                        C = data.JambRightXD(st)
                        D = data.JambRightYD(st)
                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    DrawJambLine("JambRight", MyFile, A, b, C, D)
                Next j

                element.elementType = "JambRight"
                drCoord.elements.Add(element)

                data.VERTICALMEMBER(offset + st) = "JambRight"
            Next st

            For st = 1 To 2 * (data.endOfPost)
                If data.EndPostStYB(st) < 1 Or data.EndPostStYD(st) < 1 Then
                    Continue For
                End If
                Dim element = New Element()

                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.EndPostStXA(st)
                        b = data.EndPostStYA(st)
                        C = data.EndPostStXB(st)
                        D = data.EndPostStYB(st)
                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.EndPostStXC(st)
                        b = data.EndPostStYC(st)
                        C = data.EndPostStXD(st)
                        D = data.EndPostStYD(st)
                        element.LineB = CreateLine(A, b, C, D)

                    End If
                    If j = 3 Then
                        A = data.EndPostStXA(st)
                        b = data.EndPostStYA(st)
                        C = data.EndPostStXC(st)
                        D = data.EndPostStYC(st)
                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.EndPostStXB(st)
                        b = data.EndPostStYB(st)
                        C = data.EndPostStXD(st)
                        D = data.EndPostStYD(st)
                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    If st <= data.endOfPost Then
                        DrawEndPostLine("EndPostleft", MyFile, A, b, C, D)
                    Else
                        DrawEndPostLine("EndPostRight", MyFile, A, b, C, D)
                    End If

                Next j
                element.elementType = "EndPost"
                drCoord.elements.Add(element)

                data.VERTICALMEMBER(offset + st) = "JambRight"
            Next st

            For st = 1 To data.NumberOfInteriorPost
                Dim element = New Element()

                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.InterMetdiatPostAX(st)
                        b = data.InterMetdiatPostAY(st)
                        C = data.InterMetdiatPostBX(st)
                        D = data.InterMetdiatPostBY(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.InterMetdiatPostCX(st)
                        b = data.InterMetdiatPostCY(st)
                        C = data.InterMetdiatPostDX(st)
                        D = data.InterMetdiatPostDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.InterMetdiatPostAX(st)
                        b = data.InterMetdiatPostAY(st)
                        C = data.InterMetdiatPostCX(st)
                        D = data.InterMetdiatPostCY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.InterMetdiatPostBX(st)
                        b = data.InterMetdiatPostBY(st)
                        C = data.InterMetdiatPostDX(st)
                        D = data.InterMetdiatPostDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 9
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j
                element.elementType = "AdditionalPost"
                drCoord.elements.Add(element)
            Next st

            For st = 1 To data.NumberOfOpening - 1

                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.OpeningAX(st)
                        b = data.OpeningAY(st)
                        C = data.OpeningBX(st)
                        D = data.OpeningBY(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.OpeningCX(st)
                        b = data.OpeningCY(st)
                        C = data.OpeningDX(st)
                        D = data.OpeningDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.OpeningAX(st)
                        b = data.OpeningAY(st)
                        C = data.OpeningCX(st)
                        D = data.OpeningCY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.OpeningBX(st)
                        b = data.OpeningBY(st)
                        C = data.OpeningDX(st)
                        D = data.OpeningDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                    element.elementType = "Header"
                    drCoord.elements.Add(element)
                Next j
            Next st

            For st = 1 To data.NumberOfOpening
                If data.TopTrackAYO(st) = 0 Then
                    Continue For
                End If

                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.TopTrackAXO(st)
                        b = data.TopTrackAYO(st)
                        C = data.TopTrackBXO(st)
                        D = data.TopTrackBYO(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.TopTrackCXO(st)
                        b = data.TopTrackCYO(st)
                        C = data.TopTrackDXO(st)
                        D = data.TopTrackDYO(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.TopTrackAXO(st)
                        b = data.TopTrackAYO(st)
                        C = data.TopTrackCXO(st)
                        D = data.TopTrackCYO(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.TopTrackBXO(st)
                        b = data.TopTrackBYO(st)
                        C = data.TopTrackDXO(st)
                        D = data.TopTrackDYO(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "openingTopTrack"
                drCoord.elements.Add(element)
            Next st

            For st = 1 To data.NumberOfOpening - 1

                Dim element = New Element()

                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.BottomTrackAXO(st)
                        b = data.BottomTrackAYO(st)
                        C = data.BottomTrackBXO(st)
                        D = data.BottomTrackBYO(st)


                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.BottomTrackCXO(st)
                        b = data.BottomTrackCYO(st)
                        C = data.BottomTrackDXO(st)
                        D = data.BottomTrackDYO(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.BottomTrackAXO(st)
                        b = data.BottomTrackAYO(st)
                        C = data.BottomTrackCXO(st)
                        D = data.BottomTrackCYO(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.BottomTrackBXO(st)
                        b = data.BottomTrackBYO(st)
                        C = data.BottomTrackDXO(st)
                        D = data.BottomTrackDYO(st)


                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 10
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "openingBottomTrack"
                drCoord.elements.Add(element)
            Next st

            For st = 0 To data.NumberOfSlope - 1

                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = topbktrackData.AX(st)
                        b = topbktrackData.AY(st)
                        C = topbktrackData.BX(st)
                        D = topbktrackData.BY(st)


                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = topbktrackData.CX(st)
                        b = topbktrackData.CY(st)
                        C = topbktrackData.DX(st)
                        D = topbktrackData.DY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = topbktrackData.AX(st)
                        b = topbktrackData.AY(st)
                        C = topbktrackData.CX(st)
                        D = topbktrackData.CY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = topbktrackData.BX(st)
                        b = topbktrackData.BY(st)
                        C = topbktrackData.DX(st)
                        D = topbktrackData.DY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 3
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "topTrack"

                drCoord.elements.Add(element)
            Next st

            For st = 1 To 1
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = topbktrackData.BottomTrackAX
                        b = topbktrackData.BottomTrackAY
                        C = topbktrackData.BottomTrackBX
                        D = topbktrackData.BottomTrackBY

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = topbktrackData.BottomTrackCX
                        b = topbktrackData.BottomTrackCY
                        C = topbktrackData.BottomTrackDX
                        D = topbktrackData.BottomTrackDY

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = topbktrackData.BottomTrackAX
                        b = topbktrackData.BottomTrackAY
                        C = topbktrackData.BottomTrackCX
                        D = topbktrackData.BottomTrackCY

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = topbktrackData.BottomTrackBX
                        b = topbktrackData.BottomTrackBY
                        C = topbktrackData.BottomTrackDX
                        D = topbktrackData.BottomTrackDY

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 5
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next


                element.elementType = "BottomTrack"
                drCoord.elements.Add(element)
            Next

            For st = 0 To data.NumberOfOpening - 1
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.ShearPlateLeftAX(st)
                        b = data.ShearPlateLeftAY(st)
                        C = data.ShearPlateLeftBX(st)
                        D = data.ShearPlateLeftBY(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.ShearPlateLeftCX(st)
                        b = data.ShearPlateLeftCY(st)
                        C = data.ShearPlateLeftDX(st)
                        D = data.ShearPlateLeftDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.ShearPlateLeftAX(st)
                        b = data.ShearPlateLeftAY(st)
                        C = data.ShearPlateLeftCX(st)
                        D = data.ShearPlateLeftCY(st)
                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.ShearPlateLeftBX(st)
                        b = data.ShearPlateLeftBY(st)
                        C = data.ShearPlateLeftDX(st)
                        D = data.ShearPlateLeftDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "shearPlate"
                drCoord.elements.Add(element)
            Next st

            For st = 0 To data.NumberOfOpening - 1
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.ShearPlateRightAX(st)
                        b = data.ShearPlateRightAY(st)
                        C = data.ShearPlateRightBX(st)
                        D = data.ShearPlateRightBY(st)


                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.ShearPlateRightCX(st)
                        b = data.ShearPlateRightCY(st)
                        C = data.ShearPlateRightDX(st)
                        D = data.ShearPlateRightDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.ShearPlateRightAX(st)
                        b = data.ShearPlateRightAY(st)
                        C = data.ShearPlateRightCX(st)
                        D = data.ShearPlateRightCY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.ShearPlateRightBX(st)
                        b = data.ShearPlateRightBY(st)
                        C = data.ShearPlateRightDX(st)
                        D = data.ShearPlateRightDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 11
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "shreaPlate"
                drCoord.elements.Add(element)
            Next st


            For st = 0 To hblockingNumber
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.HorizontalBlockingAX(st)
                        b = data.HorizontalBlockingAY(st)
                        C = data.HorizontalBlockingBX(st)
                        D = data.HorizontalBlockingBY(st)


                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.HorizontalBlockingCX(st)
                        b = data.HorizontalBlockingCY(st)
                        C = data.HorizontalBlockingDX(st)
                        D = data.HorizontalBlockingDY(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.HorizontalBlockingAX(st)
                        b = data.HorizontalBlockingAY(st)
                        C = data.HorizontalBlockingCX(st)
                        D = data.HorizontalBlockingCY(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.HorizontalBlockingBX(st)
                        b = data.HorizontalBlockingBY(st)
                        C = data.HorizontalBlockingDX(st)
                        D = data.HorizontalBlockingDY(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 6
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "horizontalBlocking"
                drCoord.elements.Add(element)
            Next st

            For st = 1 To numberofStrips

                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then
                        A = data.HorizontalStrapHeightAx(st)
                        b = data.HorizontalStrapHeightAy(st)
                        C = data.HorizontalStrapHeightBx(st)
                        D = data.HorizontalStrapHeightBy(st)

                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.HorizontalStrapHeightCx(st)
                        b = data.HorizontalStrapHeightCy(st)
                        C = data.HorizontalStrapHeightDx(st)
                        D = data.HorizontalStrapHeightDy(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.HorizontalStrapHeightAx(st)
                        b = data.HorizontalStrapHeightAy(st)
                        C = data.HorizontalStrapHeightCx(st)
                        D = data.HorizontalStrapHeightCy(st)

                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.HorizontalStrapHeightBx(st)
                        b = data.HorizontalStrapHeightBy(st)
                        C = data.HorizontalStrapHeightDx(st)
                        D = data.HorizontalStrapHeightDy(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 8
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "Horizontal Strap"

                drCoord.elements.Add(element)
            Next st

            For st = 0 To 2 * (data.NumberofIntersectionWall - 1) - 1
                Dim element = New Element()
                For j As Integer = 1 To 4
                    If j = 1 Then

                        A = data.IntersectionWallAX(st)
                        b = data.IntersectionWallAY(st)
                        C = data.IntersectionWallBX(st)
                        D = data.IntersectionWallBy(st)


                        element.LineA = CreateLine(A, b, C, D)
                    End If
                    If j = 2 Then
                        A = data.IntersectionWallcX(st)
                        b = data.IntersectionWallcy(st)
                        C = data.IntersectionWalldX(st)
                        D = data.IntersectionWalldy(st)

                        element.LineB = CreateLine(A, b, C, D)
                    End If
                    If j = 3 Then
                        A = data.IntersectionWallAX(st)
                        b = data.IntersectionWallAY(st)
                        C = data.IntersectionWallcX(st)
                        D = data.IntersectionWallcy(st)


                        element.LineC = CreateLine(A, b, C, D)
                    End If
                    If j = 4 Then
                        A = data.IntersectionWallBX(st)
                        b = data.IntersectionWallBy(st)
                        C = data.IntersectionWalldX(st)
                        D = data.IntersectionWalldy(st)

                        element.LineD = CreateLine(A, b, C, D)
                    End If
                    lineColor = 4
                    DRAWLINE(MyFile, lineColor, A, b, C, D)
                Next j

                element.elementType = "Intersection Wall"
                drCoord.elements.Add(element)
            Next st

EndCase:

            MyFile.WriteLine("ENDSEC")
            MyFile.WriteLine(0)
            MyFile.WriteLine("EOF")
            MyFile.Close()
        Catch ex As Exception
            Return
        End Try


    End Sub


    Public Function CreateLine(ax, ay, bx, by) As Fr_Line

        Dim line = New Fr_Line() With {
                            .pointA = New Fr_Point() With {
                                .PX = ax,
                                .PY = ay
                           },
                           .pointB = New Fr_Point() With {
                             .PX = bx,
                             .PY = by
                          }
                        }
        Return line

    End Function
End Class

