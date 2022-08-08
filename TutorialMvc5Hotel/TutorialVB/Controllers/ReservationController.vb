Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Data
Imports System.Dynamic
Imports System.Linq
Imports System.Net
Imports System.Reflection.Emit
Imports System.Security.Cryptography.X509Certificates
Imports System.Web
Imports System.Web.Mvc
Imports Data
Imports DayPilot.Web.Mvc.Json
Imports DayPilot.Web.Mvc.Recurrence
Imports TutorialCS

Public Class ReservationController
	Inherits Controller

	Public Function Edit(ByVal id As String) As ActionResult
		Dim dr As DataRow = Db.GetReservation(id)

		If dr Is Nothing Then
			Throw New Exception("The task was not found")
		End If

		Return View(New With {Key .Id = id, Key .Text = dr("ReservationName"), Key .Start = Convert.ToDateTime(dr("ReservationStart")).ToShortDateString(), Key .End = Convert.ToDateTime(dr("ReservationEnd")).ToShortDateString(), Key .Status = New SelectList(New SelectListItem() { New SelectListItem With {.Text = "New", .Value = "0"}, New SelectListItem With {.Text = "Confirmed", .Value = "1"}, New SelectListItem With {.Text = "Arrived", .Value = "2"}, New SelectListItem With {.Text = "Checked out", .Value = "3"} }, "Value", "Text", dr("ReservationStatus")), Key .Paid = New SelectList(New SelectListItem() { New SelectListItem With {.Text = "0%", .Value = "0"}, New SelectListItem With {.Text = "50%", .Value = "50"}, New SelectListItem With {.Text = "100%", .Value = "100"}}, "Value", "Text", dr("ReservationPaid")), Key .Resource = New SelectList(Db.GetRoomSelectList(), "Value", "Text", dr("RoomId"))})
	End Function

	<AcceptVerbs(HttpVerbs.Post)> _
	Public Function Edit(ByVal form As FormCollection) As ActionResult
		Dim id As String = form("Id")
		Dim name As String = form("Text")
		Dim start As Date = Convert.ToDateTime(form("Start")).Date.AddHours(12)
		Dim [end] As Date = Convert.ToDateTime(form("End")).Date.AddHours(12)
		Dim resource As String = form("Resource")
		Dim paid As Integer = Convert.ToInt32(form("Paid"))
		Dim status As Integer = Convert.ToInt32(form("Status"))

		Dim dr As DataRow = Db.GetReservation(id)

		If dr Is Nothing Then
			Throw New Exception("The task was not found")
		End If

		Db.UpdateReservation(id, name, start, [end], resource, status, paid)

		Return JavaScript(SimpleJsonSerializer.Serialize("OK"))
	End Function

	Public Function Create() As ActionResult
		Return View(New With {Key .Start = Convert.ToDateTime(Request.QueryString("start")).ToShortDateString(), Key .End = Convert.ToDateTime(Request.QueryString("end")).ToShortDateString(), Key .Resource = New SelectList(Db.GetRoomSelectList(), "Value", "Text", Request.QueryString("resource"))})
	End Function

	<AcceptVerbs(HttpVerbs.Post)> _
	Public Function Create(ByVal form As FormCollection) As ActionResult
		Dim start As Date = Convert.ToDateTime(form("Start")).Date.AddHours(12)
		Dim [end] As Date = Convert.ToDateTime(form("End")).Date.AddHours(12)
		Dim text As String = form("Text")
		Dim resource As String = form("Resource")

		Db.CreateReservation(start, [end], resource, text)
		Return JavaScript(SimpleJsonSerializer.Serialize("OK"))
	End Function


End Class
