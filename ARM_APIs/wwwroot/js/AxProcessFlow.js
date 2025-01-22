class AxProcessFlow {
    constructor() {
        this.isAxpertFlutter = !this.isNullOrEmpty(armToken);
        this.processName = '';
        this.keyField = '';
        this.keyValue = '';
        this.stepHtml = `
            <div class="step">
                <div>
                    <div class="circle ">
                        <i class="fa fa-check"></i>
                        <span class="Emp-steps-counts">{{sno}}</span>
                    </div>
                    <div class="line"></div>
                </div>
                <div class="Task-process-wrapper">
                    {{groupNameHtml}}
                    {{taskCaptionHtml}}
                </div>
            </div>`;
        this.groupNameHtml = `
            <div class="title">
                <a href="#">{{taskgroupname}}</a>
            </div>`
        this.taskCaptionHtml = `<div class="Task-process-list status-{{taskstatus}}" onclick="axProcessObj.openTask(this, '{{taskname}}', '{{tasktype}}', '{{transid}}', '{{keyfield}}', '{{keyvalue}}', '{{recordid}}');"><a href="#">{{taskname}}</a></div>`;
        this.dataSources = [];
        this.processFlowObj = {};
        this.getUrlParams();
    }

    reloadProcess(recordId) {
        ShowDimmer(true);
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/AxGetKeyValue";
        let data = { processName: this.processName, recordId: recordId };
        this.callAPI(url, data, false, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                _this.refreshProcess(dataResult[0].keyvalue);
            }
        });
    }

    refreshProcess(keyValue) {
        ShowDimmer(true);
        const params = new URLSearchParams(location.search);
        params.set('keyvalue', keyValue);
        params.toString();
        window.history.replaceState({}, '', `${location.pathname}?${params.toString()}`);

        let $rightIframe = document.querySelector("#rightIframe");
        $rightIframe.setAttribute("src", "");

        this.dataSources = [];
        this.processFlowObj = {};
        this.keyValue = keyValue;
        this.fetchProcessFlow("Process");
        this.showProcessFlow();
        this.fetchProcessKeyValues("ProcessKeyValues");
    }
    fetchProcessFlow(name) {
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/AxGetProcess";
        if (_this.isAxpertFlutter) {
            url = "../../api/v1/ARMGetProcessFlow";
        }
        let data = { processName: this.processName, keyField: this.keyField, keyValue: this.keyValue };
        this.callAPI(url, data, false, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                _this.dataSources[name] = dataResult;
            }
        });
    }

    fetchProcessKeyValues(name) {
        let _this = this;
        let url = "../../aspx/AxPEG.aspx/AxGetProcessKeyValues";
        let data = { processName: this.processName };
        this.callAPI(url, data, true, result => {
            if (result.success) {
                let json = JSON.parse(result.response);
                let dataResult = _this.dataConvert(json, "ARM");
                let dataArray = [];
                dataResult.forEach((item) => {
                    dataArray.push({ id: item.keyvalue, text: item.keyvalue })
                })
                $("select#keyvalues-select").select2({
                    placeHolder: "Search...",
                    data: dataArray
                }).on('select2:select', function (e) {
                    let keyValue = e.params.data.text;
                    _this.refreshProcess(keyValue);
                }).on('select2:open', () => {
                    $(this).find('.select2-search__field').focus();
                }).on('select2:close', () => {
                    $('.searchBoxChildContainer.search').addClass('d-none');
                });
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

    showProcessFlow() {
        this.dataSources["Process"].forEach((rowData, idx) => {
            if (this.isUndefined(this.processFlowObj[rowData.taskgroupname])) {
                this.processFlowObj[rowData.taskgroupname] = {};
                this.processFlowObj[rowData.taskgroupname].group_name_html = '';
                this.processFlowObj[rowData.taskgroupname].task_caption_html = '';

            }

            if (this.isNullOrEmpty(rowData.taskstatus) && rowData.indexno > 1) {
                rowData.taskstatus = "disabled";
            }

            if (this.isNullOrEmpty(rowData.recordid)) {
                rowData.recordid = "0";
            }

            let taskGroup = this.processFlowObj[rowData.taskgroupname];
            taskGroup.indexno = rowData.indexno;
            taskGroup.group_name_html = Handlebars.compile(this.groupNameHtml)(rowData);
            taskGroup.task_caption_html += Handlebars.compile(this.taskCaptionHtml)(rowData);

        });

        document.querySelector('#procflow-steps').innerHTML = "";
        if (this.isNullOrEmpty(this.keyValue))
            document.querySelector('#process-ref').innerText = '';
        else
            document.querySelector('#process-ref').innerText = `Identifier : ${this.keyValue}`.toUpperCase();

        let sno = 1;
        for (let [key, value] of Object.entries(this.processFlowObj)) {
            document.querySelector('#procflow-steps').insertAdjacentHTML("beforeend", ` ${this.stepHtml.replace("{{sno}}", sno).replace("{{groupNameHtml}}", value.group_name_html).replace("{{taskCaptionHtml}}", value.task_caption_html)} `);
            sno++;
        }
        ShowDimmer(false);
        let activeTask = document.querySelector('.status-active');
        if (this.isNull(activeTask)) {
            ShowDimmer(true);
            document.querySelector('.Task-process-list').click();
        }
        else {
            ShowDimmer(true);
            activeTask.click();
        }        
    }

    openTask(elem, taskName, taskType, transId, keyField, keyValue, recordId) {
        var tasks = document.querySelectorAll(".Task-process-list.Active");

        [].forEach.call(tasks, function (el) {
            el.classList.remove("Active");
        });

        elem.classList.add("Active");

        var steps = document.querySelectorAll(".step.step-active");

        [].forEach.call(steps, function (el) {
            el.classList.remove("step-active");
        });

        elem.closest(".step").classList.add("step-active");

        switch (taskType.toUpperCase()) {
            case "MAKE":
                this.openTstruct(transId, keyField, keyValue, recordId);
                break;
            case "CHECK":
                this.openProcessTask(taskName, taskType);
                break;
            case "APPROVE":
                this.openProcessTask(taskName, taskType);
                break;
        }
    }

    openTstruct(transId, keyField, keyValue, recordId) {
        ShowDimmer(true);
        let url = `../../aspx/tstruct.aspx?transid=${transId}`;
        if (recordId != "0")
            url += `&act=load&recordid=${recordId}`;
        else {
            if (keyValue != "" && keyValue != '{{keyvalue}}') {
                url += `&act=open&${keyField}=${keyValue}`
            }
            else {
                url += `&act=open&${this.keyField}=${this.keyValue}`
            }
        }

        let $rightIframe = document.querySelector("#rightIframe");
        $rightIframe.setAttribute("src", "");
        $rightIframe.setAttribute("src", url);
    }

    getUrlParams() {
        const queryString = window.location.search;
        const urlParams = new URLSearchParams(queryString);
        this.keyField = urlParams.get('keyfield');
        this.keyValue = urlParams.get('keyvalue') || '';
        this.processName = urlParams.get('processname') || _processName;
        document.querySelector('#process-name').innerText = this.processName;
    }

    openProcessTask(taskName, taskType) {
        ShowDimmer(true);
        let url = `../../aspx/htmlPages.aspx?loadcaption=AxTaskListing&processname=${this.processName}&keyfield=${this.keyField}&keyvalue=${this.keyValue}&taskname=${taskName}&tasktype=${taskType}`;
        let $rightIframe = document.querySelector("#rightIframe");
        $rightIframe.setAttribute("src", "");
        $rightIframe.setAttribute("src", url);
    }

    openSearch() {
        if ($('.searchBoxChildContainer.search').hasClass('d-none')) {
            $('.searchBoxChildContainer.search').removeClass('d-none');
            $('select#keyvalues-select').select2('open');
        }
        else {
            $('.searchBoxChildContainer.search').addClass('d-none');
            $('select#keyvalues-select').select2('close');
        }
    }
}

var axProcessObj;
$(document).ready(function () {
    ShowDimmer(true);
    axProcessObj = new AxProcessFlow();
    axProcessObj.fetchProcessFlow("Process");
    axProcessObj.showProcessFlow();    
    axProcessObj.fetchProcessKeyValues("ProcessKeyValues");    
});