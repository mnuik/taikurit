Imports System
Imports System.Data
Imports System.Drawing
Imports System.Web.Mvc
Imports Data
Imports DayPilot.Web.Mvc
Imports DayPilot.Web.Mvc.Data
Imports DayPilot.Web.Mvc.Enums
Imports DayPilot.Web.Mvc.Events.Scheduler

Namespace TutorialCS.Controllers


	Public Class SchedulerController
		Inherits Controller

		Public Function Backend() As ActionResult
			Return (New Scheduler()).CallBack(Me)
		End Function

		Private Class Scheduler
			Inherits DayPilotScheduler

			Protected Overrides Sub OnInit(ByVal e As InitArgs)
                Dim start As New Date(2018, 1, 1, 12, 0, 0)
                Dim [end] As New Date(2019, 1, 1, 12, 0, 0)

                Timeline = New TimeCellCollection()
				Dim cell As Date = start
				Do While cell < [end]
					Timeline.Add(cell, cell.AddDays(1))
					cell = cell.AddDays(1)
				Loop

				LoadRoomsAndReservations()
				ScrollTo(Date.Today.AddDays(-1))
				Separators.Add(Date.Now, Color.Red)
				UpdateWithMessage("Welcome!", CallBackUpdateType.Full)
			End Sub

			Private Sub LoadRoomsAndReservations()
				LoadRooms()
				LoadReservations()
			End Sub

			Private Sub LoadReservations()
				Events = Db.GetReservations().Rows

				DataStartField = "ReservationStart"
				DataEndField = "ReservationEnd"
				DataIdField = "ReservationId"
				DataTextField = "ReservationName"
				DataResourceField = "RoomId"

				DataTagFields = "ReservationStatus"

			End Sub

			Private Sub LoadRooms()
				Resources.Clear()

				Dim roomFilter As String = "0"
				If ClientState("filter") IsNot Nothing Then
					roomFilter = CStr(ClientState("filter")("room"))
				End If

				Dim dt As DataTable = Db.GetRoomsFiltered(roomFilter)

				For Each r As DataRow In dt.Rows
					Dim name As String = DirectCast(r("RoomName"), String)
					Dim id As String = Convert.ToString(r("RoomId"))
					Dim status As String = DirectCast(r("RoomStatus"), String)
					Dim beds As Integer = Convert.ToInt32(r("RoomSize"))
					Dim bedsFormatted As String = If(beds = 1, "1 bed", String.Format("{0} beds", beds))

					Dim res As New Resource(name, id)
					res.DataItem = r
					res.Columns.Add(New ResourceColumn(bedsFormatted))
					res.Columns.Add(New ResourceColumn(status))

					Resources.Add(res)
				Next r
			End Sub

			Protected Overrides Sub OnEventMove(ByVal e As EventMoveArgs)
				Dim id As String = e.Id
				Dim start As Date = e.NewStart
				Dim [end] As Date = e.NewEnd
				Dim resource As String = e.NewResource

				Dim message As String = Nothing
				If Not Db.IsFree(id, start, [end], resource) Then
					message = "The reservation cannot overlap with an existing reservation."
				ElseIf e.OldEnd <= Date.Today Then
					message = "This reservation cannot be changed anymore."
				ElseIf e.NewStart < Date.Today Then
					message = "The reservation cannot be moved to the past."
				Else
					Db.MoveReservation(e.Id, e.NewStart, e.NewEnd, e.NewResource)
				End If

				LoadReservations()
				UpdateWithMessage(message)
			End Sub

			Protected Overrides Sub OnEventResize(ByVal e As EventResizeArgs)
				Db.MoveReservation(e.Id, e.NewStart, e.NewEnd, e.Resource)
				LoadReservations()
				Update()
			End Sub

			Protected Overrides Sub OnBeforeEventRender(ByVal e As BeforeEventRenderArgs)
				e.Html = String.Format("{0} ({1:d} - {2:d})", e.Text, e.Start, e.End)
				Dim status As Integer = Convert.ToInt32(e.Tag("ReservationStatus"))

				Select Case status
					Case 0 ' new
						If e.Start < Date.Today.AddDays(2) Then ' must be confirmed two day in advance
							e.DurationBarColor = "red"
							e.ToolTip = "Expired (not confirmed in time)"
						Else
							e.DurationBarColor = "orange"
							e.ToolTip = "New"
						End If
					Case 1 ' confirmed
						If e.Start < Date.Today OrElse (e.Start = Date.Today AndAlso Date.Now.TimeOfDay.Hours > 18) Then ' must arrive before 6 pm
							e.DurationBarColor = "#f41616" ' red
							e.ToolTip = "Late arrival"
						Else
							e.DurationBarColor = "green"
							e.ToolTip = "Confirmed"
						End If
					Case 2 ' arrived
						If e.End < Date.Today OrElse (e.End = Date.Today AndAlso Date.Now.TimeOfDay.Hours > 11) Then ' must checkout before 10 am
							e.DurationBarColor = "#f41616" ' red
							e.ToolTip = "Late checkout"
						Else
							e.DurationBarColor = "#1691f4" ' blue
							e.ToolTip = "Arrived"
						End If
					Case 3 ' checked out
						e.DurationBarColor = "gray"
						e.ToolTip = "Checked out"
					Case Else
						Throw New ArgumentException("Unexpected status.")
				End Select

				e.Html = String.Format("<div>{0}<br /><span style='color:gray'>{1}</span></div>", e.Html, e.ToolTip)


				Dim paid As Integer = Convert.ToInt32(e.DataItem("ReservationPaid"))
				Dim paidColor As String = "#aaaaaa"

				e.Areas.Add((New Area()).Bottom(6).Right(4).Html("<div style='color:" & paidColor & "; font-size: 8pt;'>Paid: " & paid & "%</div>").Visible())
				e.Areas.Add((New Area()).Left(4).Bottom(4).Right(4).Height(2).Html("<div style='background-color:" & paidColor & "; height: 100%; width:" & paid & "%'></div>").Visible())
			End Sub

			Protected Overrides Sub OnBeforeResHeaderRender(ByVal e As BeforeResHeaderRenderArgs)
				Dim status As String = CStr(e.DataItem("RoomStatus"))
				Select Case status
					Case "Dirty"
						e.CssClass = "status_dirty"
					Case "Cleanup"
						e.CssClass = "status_cleanup"
				End Select
			End Sub


			Protected Overrides Sub OnCommand(ByVal e As CommandArgs)
				Select Case e.Command
					Case "refresh"
						LoadReservations()
						UpdateWithMessage("Refreshed")
					Case "filter"
						LoadRoomsAndReservations()
						UpdateWithMessage("Updated", CallBackUpdateType.Full)
				End Select
			End Sub


		End Class

	End Class

End Namespace
