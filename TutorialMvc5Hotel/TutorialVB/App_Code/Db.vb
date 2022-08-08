Imports System
Imports System.Collections.Generic
Imports System.Configuration
Imports System.Data
Imports System.Data.SqlClient
Imports System.Linq
Imports System.Web
Imports System.Web.Mvc

Namespace Data
	''' <summary>
	''' Summary description for Db
	''' </summary>
	Public NotInheritable Class Db

		Private Sub New()
		End Sub


		Public Shared Function GetRooms() As DataTable
			Dim da As New SqlDataAdapter("SELECT * FROM [Room] order by [RoomName]", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			Dim dt As New DataTable()
			da.Fill(dt)

			Return dt
		End Function

		Public Shared Function GetRoomSelectList() As IEnumerable(Of SelectListItem)
			Return GetRooms().AsEnumerable().Select(Function(u) New SelectListItem With {.Value = Convert.ToString(u.Field(Of Integer)("RoomId")), .Text = u.Field(Of String)("RoomName")})
		End Function

		Public Shared Function GetReservation(ByVal id As String) As DataRow
			Dim da As New SqlDataAdapter("SELECT * FROM [Reservation] WHERE [ReservationId] = @id", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			da.SelectCommand.Parameters.AddWithValue("id", id)

			Dim dt As New DataTable()
			da.Fill(dt)

			If dt.Rows.Count > 0 Then
				Return dt.Rows(0)
			End If
			Return Nothing
		End Function

		Public Shared Function GetReservations() As DataTable
			Dim da As New SqlDataAdapter("SELECT * FROM [Reservation]", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)

			Dim dt As New DataTable()
			da.Fill(dt)

			Return dt
		End Function




		Public Shared Sub MoveReservation(ByVal id As String, ByVal start As Date, ByVal [end] As Date, ByVal resource As String)
			Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
				con.Open()
				Dim cmd As New SqlCommand("UPDATE [Reservation] SET [ReservationStart] = @start, [ReservationEnd] = @end, [RoomId] = @resource WHERE [ReservationId] = @id", con)
				cmd.Parameters.AddWithValue("id", id)
				cmd.Parameters.AddWithValue("start", start)
				cmd.Parameters.AddWithValue("end", [end])
				cmd.Parameters.AddWithValue("resource", resource)
				cmd.ExecuteNonQuery()
			End Using
		End Sub

		Public Shared Sub CreateReservation(ByVal start As Date, ByVal [end] As Date, ByVal resource As String, ByVal name As String)
			Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
				con.Open()
				Dim cmd As New SqlCommand("INSERT INTO [Reservation] ([ReservationStart], [ReservationEnd], [RoomId], [ReservationName], [ReservationStatus]) VALUES (@start, @end, @resource, @name, 0)", con)
				cmd.Parameters.AddWithValue("start", start)
				cmd.Parameters.AddWithValue("end", [end])
				cmd.Parameters.AddWithValue("resource", resource)
				cmd.Parameters.AddWithValue("name", name)
				cmd.ExecuteNonQuery()
			End Using
		End Sub

		Public Shared Sub UpdateReservation(ByVal id As String, ByVal name As String, ByVal start As Date, ByVal [end] As Date, ByVal resource As String, ByVal status As Integer, ByVal paid As Integer)
			Using con As New SqlConnection(ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
				con.Open()
				Dim cmd As New SqlCommand("UPDATE [Reservation] SET [ReservationStart] = @start, [ReservationEnd] = @end, [RoomId] = @resource, [ReservationName] = @name, [ReservationStatus] = @status, [ReservationPaid] = @paid WHERE [ReservationId] = @id", con)
				cmd.Parameters.AddWithValue("id", id)
				cmd.Parameters.AddWithValue("start", start)
				cmd.Parameters.AddWithValue("end", [end])
				cmd.Parameters.AddWithValue("name", name)
				cmd.Parameters.AddWithValue("resource", resource)
				cmd.Parameters.AddWithValue("status", status)
				cmd.Parameters.AddWithValue("paid", paid)
				cmd.ExecuteNonQuery()
			End Using

		End Sub

		Public Shared Function GetRoomsFiltered(ByVal roomFilter As String) As DataTable
			Dim da As New SqlDataAdapter("SELECT [RoomId], [RoomName], [RoomStatus], [RoomSize] FROM [Room] WHERE RoomSize = @beds or @beds = '0'", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			da.SelectCommand.Parameters.AddWithValue("beds", roomFilter)
			Dim dt As New DataTable()
			da.Fill(dt)

			Return dt
		End Function

		Public Shared Function IsFree(ByVal id As String, ByVal start As Date, ByVal [end] As Date, ByVal resource As String) As Boolean
			' event with the specified id will be ignored

			Dim da As New SqlDataAdapter("SELECT count(ReservationId) as count FROM [Reservation] WHERE NOT (([ReservationEnd] <= @start) OR ([ReservationStart] >= @end)) AND RoomId = @resource AND ReservationId <> @id", ConfigurationManager.ConnectionStrings("daypilot").ConnectionString)
			da.SelectCommand.Parameters.AddWithValue("id", id)
			da.SelectCommand.Parameters.AddWithValue("start", start)
			da.SelectCommand.Parameters.AddWithValue("end", [end])
			da.SelectCommand.Parameters.AddWithValue("resource", resource)
			Dim dt As New DataTable()
			da.Fill(dt)

			Dim count As Integer = Convert.ToInt32(dt.Rows(0)("count"))
			Return count = 0
		End Function
	End Class

End Namespace
