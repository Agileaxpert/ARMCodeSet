class AxTasks {
    constructor() {
        this.isAxpertFlutter = !this.isNullOrEmpty(armToken);
        this.taskCardHtml = `
        <div class="col-md-12 pb-8 task-listing-card" data-process="{{processname}}" data-type="{{tasktype}}" data-taskid="{{taskid}}">
            <h2 class="task-heading"><input type="checkbox" value="" class="form-check-input task-list-checkbox"><a href="#" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}')"><span class="material-icons material-icons-style material-icons-1 display-icon">{{displayicon}}</span>{{displaytitle}}</a></h2>
            <div class="task-subtitle">{{displaysubtitle}}</div>
            <div class="d-flex flex-row task-date  ">
                <span title="Assigned From" class="task-assignedBy">From : {{fromuser}}</span>
                <span title="Assigned On" class="start-date">On : {{eventdatetime}}</span>
            </div>

            <div class="d-flex flex-row Task-content">
                <div class="fs-6 fw-semibold  py-2 col-md-8 task-display-content">
                    {{displaycontent}}
                </div>

                <div class="fs-6 fw-semibold text-gray-600 py-2 col-md-4 status-btn-wrap">
                    <div class="btn-group  approval-radio-group d-none" role="group" aria-label="Basic radio toggle button group">
                        <input type="radio" class="btn-check approval-radio" data-tasktype="Approve" name="approval{{taskid}}" id="approve{{taskid}}" autocomplete="off">
                        <label class="btn-toggle btn" for="approve{{taskid}}"><span class="material-icons material-icons-style material-icons-2" style="color: #47BE7D;">check_circle</span>Approve</label>

                        <input type="radio" class="btn-check approval-radio " data-tasktype="Reject" name="approval{{taskid}}" id="reject{{taskid}}" autocomplete="off">
                        <label class="btn-toggle btn " for="reject{{taskid}}"><span class="material-icons material-icons-style material-icons-2" style="color: red;">cancel</span>Reject</label>

                        <input type="radio" class="btn-check approval-radio" data-tasktype="Return" name="approval{{taskid}}" id="return{{taskid}}" autocomplete="off">
                        <label class="btn-toggle btn " for="return{{taskid}}"><span class="material-icons material-icons-style material-icons-2" style="color: blueviolet;">reply</span>Return</label>
                    </div>

                </div>

            </div>

            
            <div class="text-end task-more-btns">
                <div class="d-flex align-items-center  task-more-btns">
                    {{task_buttons_html}}
                </div>
            </div>
        </div>`;
        this.taskApprovalCardHtml = `
        <div class="d-flex flex-row Task-Reasons approval-reasons approval{{taskid}} d-none" data-tasktype="Approve" data-taskid="{{taskid}}">
            <div class="col-md-4 pe-5">
                <label class="form-label col-form-label   required">Approve reasons</label>
                <div class="input-group">
                    <select data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-select " data-tasktype="Approve"><option value="">NA</option></select>
                </div>
            </div>


            <div class="col-md-6 pe-5">
                <label class="form-label col-form-label  required">Comments</label>
                <div class="input-group">
                    <textarea data-tasktype="Approve" data-taskid="APPROVE{{taskid}}" class="form-control" data-tasktype="Approve"></textarea>
                </div>
            </div>

            <div class=" col-md-2 d-flex align-items-end">
                <button class="btn btn-sm btn-primary task-submit-btn  " onclick="axTasksObj.doApprove('{{taskid}}')" title="Approve">Approve</button>
            </div>
        </div>        

        <div class="d-flex flex-row Task-Reasons approval-reasons approval{{taskid}} d-none" data-tasktype="Reject" data-taskid="{{taskid}}">
            <div class="col-md-4 pe-5">
                <label class="form-label col-form-label   required">Reject reasons</label>
                <div class="input-group">
                    <select data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-select " data-tasktype="Reject"><option value="">NA</option></select>
                </div>
            </div>


            <div class="col-md-6 pe-5">
                <label class="form-label col-form-label  required">Comments</label>
                <div class="input-group">
                    <textarea data-tasktype="Reject" data-taskid="REJECT{{taskid}}" class="form-control" data-tasktype="Reject"></textarea>
                </div>
            </div>

            <div class=" col-md-2 d-flex align-items-end">
                <button class="btn btn-sm btn-primary task-submit-btn  " onclick="axTasksObj.doReject('{{taskid}}')" title="Reject">Reject</button>
            </div>
        </div>

        <div class="d-flex flex-row Task-Reasons approval-reasons approval{{taskid}} d-none" data-tasktype="Return" data-taskid="{{taskid}}">
            <div class="col-md-4 pe-5">
                <label class="form-label col-form-label   required">Return reasons</label>
                <div class="input-group">
                    <select data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-select " data-tasktype="Return"><option value="">NA</option></select>
                </div>
            </div>

            <div class="col-md-6 pe-5">
                <label class="form-label col-form-label  required">Comments</label>
                <div class="input-group">
                    <textarea data-tasktype="Return" data-taskid="RETURN{{taskid}}" class="form-control" data-tasktype="Return"></textarea>
                </div>
            </div>

            <div class=" col-md-2 d-flex align-items-end">
                <button class="btn btn-sm btn-primary task-submit-btn  " onclick="axTasksObj.doReturn('{{taskid}}')" title="Return">Return</button>
            </div>
        </div>`;
        this.taskNoRecordsCardHtml = `
        <table class="table align-middle  fs-6 gy-5 mb-0 dataTable no-footer task-listing-card">
            <tbody>
                <tr class="d-flex header">                                           
                    <td class="d-flex align-items-center task-name">
                        No active tasks available.
                    </td>
                </tr>
            </tbody>
        </table>`;
        this.taskProcessedCardHtml = `
        <table class="table align-middle  fs-6 gy-5 mb-0 dataTable no-footer task-listing-card">
            <tbody>
                <tr class="d-flex header">                                           
                    <td class="d-flex align-items-center task-name">
                        This task has been processed.
                    </td>
                </tr>
            </tbody>
        </table>`;

        this.filters = {};
        this.filters.allBtnHtml = `<span class="material-icons material-icons-style material-icons-1">apps</span>
                            All Tasks : `;
        this.filters.processFilterCardHtml = `
        <div class="accordion accordion-icon-toggle" id="" data-process="{{processname}}">
            <div class="accordion-header py-3 d-flex collapsed sub-task-lists-wrap"
                    data-bs-toggle="collapse" data-bs-target="#Task_list-{{processid}}"
                    aria-expanded="false" onclick="axTasksObj.filterTasks('process','{{processname}}',this)">
                <span class="accordion-icon">
                    <span class="material-icons material-icons-style material-icons-2">chevron_right</span>
                </span>
                <div class="  task_activity_content">
                    <div class="d-flex justify-content-center  ">
                        <div class="symbol symbol-40px symbol-circle me-5">
                            <div class="symbol-label bgs{{sno}}">
                                <span class="material-icons material-icons-style material-icons-2">account_tree</span>
                            </div>
                        </div>
                        <div class="task-process-selection">
                            <a href="#" class="task_activity-counts">
                                <div class="symbol-label">{{processcount}}</div>
                            </a>
                            <span class="task_activity-name  d-block ">{{processname}}</span>
                        </div>
                    </div>
                </div>
            </div>
            <div id="Task_list-{{processid}}" class="fs-6 ps-10 accordion-body collapse" data-process="{{processname}}">                
            </div>
        </div>`
        this.filters.typeFilterCardHtml = `
        <div class="step sub-task-lists " onclick="axTasksObj.filterTasks('type','{{processname}}~~{{type}}',this)">
            <div> <span
                    class="material-icons material-icons-style material-icons-2">{{icon}}</span>
            </div>
            <div class="title">{{type}} <span class="task-count-sub">{{count}}</span></div>
        </div>`
        this.filters.viewAllBtnHtml = `<!--<button class="task-activity-all btn btn-white" id="viewAllFilters" onclick="axTasksObj.viewAllFilters();" title="View All">View All</button>-->`;

        this.taskBtnsHtml = {};
        this.taskBtnsHtml.make = `
            <a href="#" title="Make" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2" onclick="axTasksObj.openProcess('{{processname}}','{{keyfield}}','{{keyvalue}}')""><span class="material-icons material-icons-style material-icons-2" style="color: orange;">assignment</span></a>`;
        this.taskBtnsHtml.view = `
            <a href="#" title="View form" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2" onclick="axTasksObj.openTstruct('{{transid}}','{{recordid}}')""><span class="material-icons material-icons-style material-icons-2" style="color: darkmagenta;">visibility</span></a>`;
        this.taskBtnsHtml.history = `
            <a href="#" title="View History" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2" onclick="axTasksObj.openHistory('{{processname}}','{{keyfield}}','{{keyvalue}}')""><span class="material-icons material-icons-style material-icons-2" style="color: blue;">history</span></a>`;
        this.taskBtnsHtml.approve = `
            <a href="#" title="Approve" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  btn-sm  me-2" onclick="axTasksObj.doApprove('{{taskid}}')"><span class="material-icons material-icons-style material-icons-2" style="color: #47BE7D;">check_circle</span></a>`;
        this.taskBtnsHtml.reject = `
            <a href="#" title="Reject" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm" onclick="axTasksObj.doReject('{{taskid}}')"><span class="material-icons material-icons-style material-icons-2" style="color: red;">cancel</span></a>`;
        this.taskBtnsHtml.return = `
            <a href="#" title="Return" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm" onclick="axTasksObj.doReturn('{{taskid}}')"><span class="material-icons material-icons-style material-icons-2" style="color: blueviolet;">reply</span></a>`;
        this.taskBtnsHtml.check = `
             <a href="#" title="Check" class="btn btn-white btn-color-gray-700 btn-active-primary d-inline-flex align-items-center  me-2  btn-sm" onclick="axTasksObj.doCheck('{{taskid}}')"><span class="material-icons material-icons-style material-icons-2" style="color: #47BE7D;">done_all</span></a>`;
        this.taskBtnsHtml.more = `
            <button href="#" class="btn btn-icon btn-active-light-primary w-30px h-30px"
                data-kt-menu-trigger="{default:'click', lg: 'hover'}" data-kt-menu-attach="parent"
                data-kt-menu-placement="bottom-end">
                <span class="material-icons material-icons-style material-icons-1">
                    more_horiz
                </span>
            </button>
            <div class="menu menu-sub menu-sub-dropdown menu-column menu-rounded menu-title-gray-700 menu-icon-muted menu-active-bg menu-state-primary fw-bold py-4 fs-6 w-150px"
                data-kt-menu="true" style="">

                <div class="menu-item px-3">
                    <a href="#" class="menu-link d-flex px-5 ">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                ios_share
                            </span>
                        </span>Open</a>
                </div>

                <div class="menu-item px-3">
                    <a href="#" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                check_circle
                            </span>
                        </span>Approve</a>
                </div>

                <div class="menu-item px-3">
                    <a href="#" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                cancel
                            </span>
                        </span>Reject </a>
                </div>

                <div class="menu-item px-3">
                    <a href="#" class="menu-link d-flex px-5">
                        <span class="symbol symbol-20px me-4">
                            <span class="material-icons material-icons-style material-icons-1">
                                reply
                            </span>
                        </span>Return </a>
                </div>
            </div>`;
        this.dataSources = [];
        this.processName = '';
        this.task = {};
        this.task.keyField = '';
        this.task.keyValue = '';
        this.task.taskName = '';
        this.task.taskType = '';
        //this.task.taskId = '';
    }

    fetchActiveTasks(name) {
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/GetActiveTasks";
        let data = {};        
        if (_this.isAxpertFlutter) {
            url = "../../api/v1/ARMGetActiveTasks";
        }
        this.callAPI(url, data, false, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                _this.dataSources[name] = dataResult;
            }
        });
    }

    fetchProcessTask(name) {
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/AxGetProcessTask";
        let _task = this.task;
        let data = { processName: this.processName, keyField: _task.keyField, keyValue: _task.keyValue, taskName: _task.taskName, taskType: _task.taskType };
        this.callAPI(url, data, false, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                _this.dataSources[name] = dataResult;
            }
        });
    }

    doApprove(taskId) {
        this.doTaskAction('APPROVE', taskId, 'APPROVE');
    }

    doReject(taskId) {
        this.doTaskAction('REJECT', taskId, 'APPROVE');
    }

    doReturn(taskId) {
        this.doTaskAction('RETURN', taskId, 'APPROVE');
    }

    doCheck(taskId) {
        this.doTaskAction('CHECK', taskId, 'CHECK');
    }

    doTaskAction(action, taskId, taskType) {
        ShowDimmer(true);
        let url = "../../aspx/AxPEG.aspx/AxDoTaskAction";
        let taskReason = document.querySelector(`select[data-taskid="${action}${taskId}"]`)?.value || '';
        let taskText = document.querySelector(`textarea[data-taskid="${action}${taskId}"]`)?.value || '';
        let data = { action: action, taskId: taskId, taskType: taskType, taskReason: taskReason, taskText: taskText };
        this.callAPI(url, data, true, result => {
            ShowDimmer(false);
            if (result.success) {
                let json = JSON.parse(result.response);
                json = JSON.parse(json.d);
                if (json.result.success) {
                    this.showSuccess(json.result.message);
                    if (!this.isUndefined(parent.axProcessObj))
                        parent.axProcessObj.refreshProcess(parent.axProcessObj.keyValue);
                    else
                        window.location.reload();
                }
                else {
                    this.catchError(json.result.message);
                }
            }
        });
    }

    callAPI(url, data, async, callBack) {
        let _this = this;
        var xhr = new XMLHttpRequest();
        xhr.open("POST", url, async);
        xhr.setRequestHeader("Content-Type", "application/json;charset=UTF-8");
        if (_this.isAxpertFlutter) {
            xhr.setRequestHeader('Authorization', `Bearer ${armToken}`);
            data["armSessionId"] = armSessionId;
        }

        xhr.onreadystatechange = function () {
            if (this.readyState == 4) {
                if (this.status == 200) {
                    callBack({ success: true, response: this.responseText });
                }
                else {
                    _this.catchError(this.responseText);
                    callBack({ success: false, response: this.responseText });
                }
            }
        }
        xhr.send(JSON.stringify(data));
    }

    catchError(error) {
        showAlertDialog("error", error);
    };

    showSuccess(message) {
        showAlertDialog("success", message);
    };

    isEmpty(elem) {
        return elem == "";
    };

    isNull(elem) {
        return elem == null;
    };

    isNullOrEmpty(elem) {
        return elem == null || elem == "";
    };

    isUndefined(elem) {
        return typeof elem == "undefined";
    };

    dataConvert(data, type) {
        if (type == "AXPERT") {
            try {
                data = JSON.parse(data.d);
                if (typeof data.result[0].result.row != "undefined") {
                    return data.result[0].result.row;
                }
            }
            catch (error) {
                this.catchError(error.message);
            };

            try {
                if (typeof data.result[0].result != "undefined") {
                    return data.result[0].result;
                }
            }
            catch (error) {
                this.catchError(error.message);
            };
        }
        else if (type == "ARM") {
            try {
                if (!this.isAxpertFlutter)
                    data = JSON.parse(data.d);
                if (data.result.success) {
                    if (!this.isUndefined(data.result.data)) {
                        return JSON.parse(data.result.data);
                    }
                }
                else {
                    if (!this.isUndefined(data.result.message)) {
                        this.catchError(data.result.message);
                    }
                }
            }
            catch (error) {
                this.catchError(error.message);
            };
        }

        return data;
    }

    generateFldId() {
        return `fid${Date.now()}${Math.floor(Math.random() * 90000) + 10000}`;
    };

    showTasks() {
        if (!this.dataSources.hasOwnProperty('TaskReasons')) {
            this.dataSources["TaskReasons"] = {};
        }

        if (this.dataSources["Tasks"].length == 0) {
            if (this.isUndefined(parent.axProcessObj))
                document.querySelector('#tasks_container').insertAdjacentHTML("beforeend", ` ${this.taskNoRecordsCardHtml} `);
            else
                document.querySelector('#tasks_container').insertAdjacentHTML("beforeend", ` ${this.taskProcessedCardHtml} `);
        }
        else {
            this.dataSources["Tasks"].forEach((rowData, idx) => {
                switch (rowData.tasktype.toUpperCase()) {
                    case "MAKE":
                        rowData.task_buttons_html = this.taskBtnsHtml.make;
                        break;
                    case "CHECK":
                        rowData.task_buttons_html = this.taskBtnsHtml.check;
                        break;
                    case "APPROVE":
                        rowData.task_buttons_html = "";
                        break;
                }

                this.dataSources.TaskReasons[rowData.taskid] = { approvereasons: rowData.approvereasons, defapptext: rowData.defapptext, returnreasons: rowData.returnreasons, defrettext: rowData.defrettext, rejectreasons: rowData.rejectreasons, defregtext: rowData.defregtext };

                rowData.task_buttons_html += this.taskBtnsHtml.view + this.taskBtnsHtml.history;
                //rowData.task_buttons_html = `<div class="d-flex align-items-center task-more-btns">${rowData.task_buttons_html}</div>`;
                let htmlText = Handlebars.compile(this.taskCardHtml.replace("{{task_buttons_html}}", rowData.task_buttons_html))(rowData);

                document.querySelector('#tasks_container').insertAdjacentHTML("beforeend", ` ${htmlText} `);

                if (rowData.tasktype.toUpperCase() == "APPROVE") {
                    document.querySelector(`div[data-taskid="${rowData.taskid}"] .Task-content .approval-radio-group`).classList.remove('d-none');
                    let approvalText = Handlebars.compile(this.taskApprovalCardHtml)(rowData);
                    document.querySelector(`div[data-taskid="${rowData.taskid}"] .Task-content`).insertAdjacentHTML("afterend", ` ${approvalText} `);
                    if (!this.isNullOrEmpty(rowData.approvereasons)) {
                        var approvereasons = rowData.approvereasons.split(',');
                        var select = document.querySelector(`[data-tasktype="Approve"][data-taskid="${rowData.taskid}"] select`)

                        while (select.options.length > 0) {
                            select.remove(0);
                        } 
                        for (var i = 0; i < approvereasons.length; i++) {
                            var opt = document.createElement('option');
                            opt.value = approvereasons[i];
                            opt.innerHTML = approvereasons[i];
                            select.appendChild(opt);
                        }
                    }

                    if (!this.isNullOrEmpty(rowData.defapptext)) {
                        document.querySelector(`[data-tasktype="Approve"][data-taskid="${rowData.taskid}"] textarea`).value = rowData.defapptext;
                    }

                    if (!this.isNullOrEmpty(rowData.returnreasons)) {
                        var returnreasons = rowData.returnreasons.split(',');
                        var select = document.querySelector(`[data-tasktype="Return"][data-taskid="${rowData.taskid}"] select`)
                        while (select.options.length > 0) {
                            select.remove(0);
                        } 
                        for (var i = 0; i < returnreasons.length; i++) {
                            var opt = document.createElement('option');
                            opt.value = returnreasons[i];
                            opt.innerHTML = returnreasons[i];
                            select.appendChild(opt);
                        }
                    }

                    if (!this.isNullOrEmpty(rowData.defrettext)) {
                        document.querySelector(`[data-tasktype="Return"][data-taskid="${rowData.taskid}"] textarea`).value = rowData.defrettext;
                    }


                    if (!this.isNullOrEmpty(rowData.rejectreasons)) {
                        var rejectreasons = rowData.rejectreasons.split(',');
                        var select = document.querySelector(`[data-tasktype="Reject"][data-taskid="${rowData.taskid}"] select`)
                        while (select.options.length > 0) {
                            select.remove(0);
                        } 
                        for (var i = 0; i < rejectreasons.length; i++) {
                            var opt = document.createElement('option');
                            opt.value = rejectreasons[i];
                            opt.innerHTML = rejectreasons[i];
                            select.appendChild(opt);
                        }
                    }

                    if (!this.isNullOrEmpty(rowData.defregtext)) {
                        document.querySelector(`[data-tasktype="Reject"][data-taskid="${rowData.taskid}"] textarea`).value = rowData.defregtext;
                    }
                }

            });
        }

        /*var showChar = 20;  // How many characters are shown by default
        var ellipsesText = "...";
        var moreText = "Show more >";
        var lessText = "Show less";

        document.querySelectorAll('.more').forEach((taskContent) => {
            taskContent.classList.add('hideContent');
            taskContent.insertAdjacentHTML("afterend", ` <span class="moreellipses">${ellipsesText}&nbsp;</span><span class="morecontent">&nbsp;&nbsp;<a href="#" class="morelink show-more">${moreText}</a></span>`);
        });

        $(".show-more").on("click", function () {
            var $this = $(this);
            var $content = $this.closest('.task-description').find(".more");
            var linkText = $this.text().toUpperCase();

            if (linkText === moreText.toUpperCase()) {
                linkText = lessText;
                $content.toggleClass("hideContent", "showContent", 400);
            } else {
                linkText = moreText;
                $content.toggleClass("showContent", "hideContent", 400);
            };

            $this.text(linkText);
        });*/

        //$('.more').each(function () {
        //    var content = $(this).html();

        //    if (content.length > showChar) {

        //        var c = content.substr(0, showChar);
        //        var h = content.substr(showChar, content.length - showChar);

        //        var html = c + '<span class="moreellipses">' + ellipsesText + '&nbsp;</span><span class="morecontent"><span>' + h + '</span>&nbsp;&nbsp;<a href="" class="morelink">' + moreText + '</a></span>';

        //        $(this).html(html);
        //    }

        //});

        //$(".morelink").click(function () {
        //    if ($(this).hasClass("less")) {
        //        $(this).removeClass("less");
        //        $(this).html(moreText);
        //    } else {
        //        $(this).addClass("less");
        //        $(this).html(lessText);
        //    }
        //    $(this).parent().prev().toggle();
        //    $(this).prev().toggle();
        //    return false;
        //});

        this.constructFilters();
        this.setApprovalReasons();
    }

    constructFilters() {
        let btnAll = document.querySelector("#All-tasks-count");
        if (!this.isUndefined(btnAll) && !this.isNull(btnAll)) {
            btnAll.innerHTML = this.dataSources["Tasks"].length.toString();
        }
        if (this.dataSources["Tasks"].length > 0) {
            var processWiseCounts = axTasksObj.dataSources["Tasks"].reduce((p, c) => {
                var name = c.processname;
                if (!p.hasOwnProperty(name)) {
                    p[name] = 0;
                }
                p[name]++;
                return p;
            }, {});

            let tempSno = 1;
            for (const [key, value] of Object.entries(processWiseCounts)) {
                let rowData = { processname: key, processid: this.sanitizeElementId(key), processcount: value, sno: tempSno };
                var filterHtml = Handlebars.compile(this.filters.processFilterCardHtml)(rowData);
                document.querySelector("#Active-List-container").insertAdjacentHTML("beforeend", ` ${filterHtml} `);
                if (tempSno == 6)
                    tempSno = 1;
                else
                    tempSno++;
            }
            //document.querySelector("#Active-List-container").insertAdjacentHTML("beforeend", ` ${this.filters.viewAllBtnHtml} `);

            var processWiseCounts = {}
            axTasksObj.dataSources["Tasks"].forEach((row) => {

                if (!processWiseCounts.hasOwnProperty(row.processname)) {
                    processWiseCounts[row.processname] = {};
                }

                if (!processWiseCounts[row.processname].hasOwnProperty(row.tasktype)) {
                    processWiseCounts[row.processname][row.tasktype] = 0;
                }

                processWiseCounts[row.processname][row.tasktype]++;
            });

            for (const [processname, types] of Object.entries(processWiseCounts)) {
                for (const [type, count] of Object.entries(types)) {
                    let rowData = { type: type, count: count, processname: processname, icon: this.getTaskTypeIcon(type)};
                    var filterHtml = Handlebars.compile(this.filters.typeFilterCardHtml)(rowData);
                    document.querySelector(`.accordion-body[data-process="${processname}"]`).insertAdjacentHTML("beforeend", ` ${filterHtml} `);
                }
            }

            KTMenu.createInstances('[data-kt-menu="true"]')
        }
    }

    sanitizeElementId(id) {
        return id.replace(/[^a-zA-Z0-9-_]/g, '');
    }

    getTaskTypeIcon(taskType) {
        let result = "assignment";
        switch (taskType.toUpperCase()) {            
            case "MAKE":
                result = "assignment";
                break;
            case "CHECK":
                result = "done_all";
                break;
            case "APPROVE":
                result = "check_circle";
                break;
            case "REJECT":
                result = "cancel";
                break;
            case "RETURN":
                result = "reply";
                break;
            default:
                result = "assignment";
                break;
        }
        return result;
    }

    openTstruct(transId,recordId) {
        ShowDimmer(true);
        let url = `../../aspx/tstruct.aspx?transid=${transId}`;
        if (!this.isNullOrEmpty(recordId) && recordId != "0")
            url += `&act=load&recordid=${recordId}`;
        else {
            showAlertDialog("error", "Form details not available.");
            return;
        }

        try {
            let modalObj = {
                "id": `ldb${transId}${recordId}`,
                "url": url
            };
            this.openModal(modalObj);
        } catch (error) {
            showAlertDialog("error", error.message);
        }        
    }

    openHistory(processName, keyField, keyValue) {
        let url = `../../aspx/ivtoivload.aspx?ivname=pegtaskh&pkeyvalue=${keyValue}&pprocess=${processName}`;
        try {
            let modalObj = {
                "id": `ldb${keyValue}`,
                "url": url
            };
            this.openModal(modalObj);
        } catch (error) {
            showAlertDialog("error", error.message);
        }
    }

    openModal(modalObj) {
        try {
            modalObj.iFrameModalBody = `<iframe id="${modalObj.id}" name="${modalObj.id}" class="col-12 flex-column-fluid w-100 h-100" src="${modalObj.url}" frameborder="0" allowtransparency="True"></iframe>`

            let myModal = new BSModal(`modal_${modalObj.id}`, "", modalObj.iFrameModalBody,
                (opening) => { ShowDimmer(false); }, (closing) => { }
            );

            myModal.changeSize("fullscreen");
            myModal.hideHeader();
            myModal.hideFooter();
            myModal.showFloatingClose();
        } catch (error) {
            showAlertDialog("error", error.message);
        }
    }

    getUrlParams() {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        this.task.keyField = urlParams.get('keyfield');
        this.task.keyValue = urlParams.get('keyvalue');
        this.processName = urlParams.get('processname');
        this.task.taskType = urlParams.get('tasktype');
        //this.task.taskId = urlParams.get('taskid');
        this.task.taskName = urlParams.get('taskname');
    }

    openProcess(processName, keyField, keyValue) {
        ShowDimmer(true);
        if (!this.isAxpertFlutter){
            callParentNew(`LoadIframe(htmlPages.aspx?loadcaption=AxProcessFlow&processname=${processName}&keyfield=${keyField}&keyvalue=${keyValue})`, "function");
        }
        else {
            window.location.href = `/api/v1/ARMGetAxHTML?page=AxProcessFlow.html&processname=${processName}&keyfield=${keyField}&keyvalue=${keyValue}&session=${armSessionId}`;
        }
    }

    filterTasks(type, value, elem) {
        if (type == "process") {
            document.querySelectorAll(".task-listing-card").forEach((task) => {
                if (task.dataset.process == value) {
                    task.classList.remove('d-none');
                }
                else {
                    task.classList.remove('d-none');
                    task.classList.add('d-none');
                }
            })

            document.querySelectorAll(".task-process-selection.active").forEach((filter) => {
                filter.classList.remove('active');
            })

            document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
                filter.classList.remove('active');
            })

            elem.querySelectorAll(".task-process-selection").forEach((title) => {
                title.classList.add('active');
            })
        }
        else if (type == "type") {
            let tempProcess = value.split("~~")[0];
            let tempType = value.split("~~")[1];

            document.querySelectorAll(".task-listing-card").forEach((task) => {
                if (task.dataset.type == tempType && task.dataset.process == tempProcess) {
                    task.classList.remove('d-none');
                }
                else {
                    task.classList.remove('d-none');
                    task.classList.add('d-none');
                }
            })
            document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
                filter.classList.remove('active');
            })
            elem.classList.add('active');
        }
        else if (type == "all") {
            document.querySelectorAll(".task-listing-card").forEach((task) => {
                task.classList.remove('d-none');
                task.classList.remove('active');
            })

            document.querySelectorAll(".task-process-selection.active").forEach((filter) => {
                filter.classList.remove('active');
            })
            document.querySelectorAll(".sub-task-lists.active").forEach((filter) => {
                filter.classList.remove('active');
            })
            elem.querySelectorAll(".task-process-selection").forEach((title) => {
                title.classList.add('active');
            })
        }

    }

    viewAllFilters() {
        let expanded = event.target.classList.contains('expanded');
        if (!expanded) {
            document.querySelector('#Active-List-container').classList.remove('horizontal');
            event.target.setAttribute('title', 'Collapse');
            event.target.innerHTML = 'Collapse';
            event.target.classList.add('expanded');
        }
        else {
            document.querySelector('#Active-List-container').classList.add('horizontal');
            event.target.setAttribute('title', 'View All');
            event.target.innerHTML = 'View All';
            event.target.classList.remove('expanded');
        }
    }

    setApprovalReasons() {
        var radios = document.querySelectorAll(".approval-radio");
        for (var i = 0; i < radios.length; i++) {
            radios[i].addEventListener('change', function () {
                var name = this.getAttribute('name');
                var id = this.getAttribute('id');
                document.querySelectorAll(`.${name}`).forEach((tr) => {
                    tr.classList.remove('d-none');
                    tr.classList.remove('active');
                    tr.classList.add('d-none');
                })

                this.closest('.approval-radio-group')?.querySelectorAll(`.btn-toggle`)?.forEach((item) => {
                    item.classList.remove('active');
                })

                if (this.checked) {
                    document.querySelector(`.${name}[data-tasktype="${this.dataset.tasktype}"]`)?.classList?.remove('d-none');
                    document.querySelector(`.${name}[data-tasktype="${this.dataset.tasktype}"]`)?.classList?.add('active');
                    document.querySelector(`[for="${id}"]`).classList.remove("active");
                    document.querySelector(`[for="${id}"]`).classList.add("active");
                }
            });
        }
    }
}

var axTasksObj;
$(document).ready(function () {
    ShowDimmer(true);
    axTasksObj = new AxTasks();
    axTasksObj.getUrlParams();
    let _tasks = axTasksObj.task;
    if (axTasksObj.isNullOrEmpty(_tasks.taskName) || axTasksObj.isUndefined(_tasks.taskName)) {
        axTasksObj.fetchActiveTasks("Tasks");
    }
    else {
        axTasksObj.fetchProcessTask("Tasks");
        document.querySelector("#ActiveTask_Left").classList.remove('d-flex');
        document.querySelector("#ActiveTask_Left").style.display = "none";
        document.querySelector("#add-task").style.display = "none";
        if (!axTasksObj.isUndefined(axTasksObj.dataSources?.["Tasks"]?.[0]?.["displaytitle"])) {
            document.querySelector("#task-page-title").innerHTML = axTasksObj.dataSources["Tasks"][0]["displaytitle"];
        }
    }
    axTasksObj.showTasks();
    ShowDimmer(false);
    if (typeof parent.axProcessObj != "undefined") {
        parent.ShowDimmer(false);
    }

    $("#AllTasks").click(function () {
        var filterBox = document.querySelector(".task-activity-container");
        if (filterBox.classList.contains('d-none')) {
            filterBox.classList.remove('d-none');
        }
        else {
            filterBox.classList.add('d-none');
        }

        document.getElementById('task-page-title').click();
    });
});
