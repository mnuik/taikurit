﻿@imports System.ServiceModel
@imports DayPilot.Web.Mvc
@imports DayPilot.Web.Mvc.Enums
@imports DayPilot.Web.Mvc.Events.Scheduler

@Code
    ViewBag.Title = "ASP.NET MVC Hotel Room Booking"
End Code

<style type="text/css">

    .scheduler_default_rowheader_inner {
        border-right: 1px solid #ccc;
    }

    .scheduler_default_rowheadercol2 {
        background: White;
    }

    .scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner {
        top: 2px;
        bottom: 2px;
        left: 2px;
        background-color: transparent;
        border-left: 5px solid #1a9d13; /* green */
        border-right: 0px none;
    }

    .status_dirty.scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner {
        border-left: 5px solid #ea3624; /* red */
    }

    .status_cleanup.scheduler_default_rowheadercol2 .scheduler_default_rowheader_inner {
        border-left: 5px solid #f9ba25; /* orange */
    }
</style>

<div style="margin-bottom: 20px;">    
    Show rooms:
    @Html.DropDownList("Filter", New SelectListItem() { 
            New SelectListItem() With { .Text = "All", .Value = "0" },
            New SelectListItem() With { .Text = "Single", .Value = "1" },
            New SelectListItem() With { .Text = "Double", .Value = "2" },
            New SelectListItem() With { .Text = "Triple", .Value = "3" },
            New SelectListItem() With { .Text = "Family", .Value = "4" }
        },
        New With { Key .onchange = "filter('room', this.value)" }
    )
</div>


@Html.DayPilotScheduler("dp", New DayPilotSchedulerConfig With
{
    .BackendUrl = Url.Action("Backend", "Scheduler"),
    .Scale = TimeScale.Manual,    
    .EventHeight = 80,
    .TimeRangeSelectedHandling = TimeRangeSelectedHandlingType.JavaScript,
    .TimeRangeSelectedJavaScript = "create(start, end, resource);",
    .TimeRangeSelectingJavaScript = "selecting(args)",
    .EventClickHandling = EventClickHandlingType.JavaScript,
    .EventClickJavaScript = "edit(e);",    
    .EventMoveHandling = EventMoveHandlingType.CallBack,
    .EventResizeHandling = EventResizeHandlingType.CallBack,    
    .TimeHeaders = New TimeHeaderCollection()  From  {
        New TimeHeader(GroupBy.Month),
        New TimeHeader(GroupBy.Day)
    },
    .HeaderColumns = new RowHeaderColumnCollection()  From  {
        New RowHeaderColumn("Room", 80),
        New RowHeaderColumn("Size", 80),
        New RowHeaderColumn("Status", 80)
    }
})

<script>
    function modal() {
        var m = new DayPilot.Modal();
        m.closed = function () {
            dp.clearSelection();
            var data = this.result;
            if (data == "OK") {
                dp.commandCallBack("refresh");
            }

        };
        return m;
    }

    function create(start, end, resource) {
        modal().showUrl('@Url.Action("Create", "Reservation")?start=' + start + "&end=" + end + "&resource=" + resource);
    }
    function edit(e) {
        modal().showUrl('@Url.Action("Edit", "Reservation")?id=' + e.id());
    }

    function filter(property, value) {
        if (!dp.clientState.filter) {
            dp.clientState.filter = {};
        }
        if (dp.clientState.filter[property] != value) { // only refresh when the value has changed
            dp.clientState.filter[property] = value;
            dp.commandCallBack('filter');
        }
    }

    function selecting(args) {
        var duration = Math.floor(new DayPilot.Duration(args.end.getTime() - args.start.getTime()).totalDays());
        var s = duration > 1 ? "s" : "";

        args.left.enabled = true;
        args.left.html = "Start:<br/>" + args.start.toString("M/d/yyyy");
        args.right.enabled = true;
        args.right.html = "End:<br/>" + args.end.toString("M/d/yyyy") + "<br/>" + duration + " night" + s;
    }


</script>