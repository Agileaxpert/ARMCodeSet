using ARMCommon.Model;
using StackExchange.Redis;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System;

namespace ARMCommon.Helpers
{
    public class Constants_SQL
    {
        public const string ARMGETMENUQUERY = @"SELECT replace(replace(COALESCE(h.caption, ''::text) || COALESCE('\'::text || g.caption::text, ''::text), '\\\'::text, '\'::text), '\\'::text, '\'::text) AS menupath,
             g.name, g.caption,g.icon,g.pagetype,g.img,g.type,g.parent,g.visible,g.visible,g.levelno,g.updatedby,g.createdon,g.importedon,g.createdby,g.updatedby,g.category  FROM axpages g
     LEFT JOIN (SELECT COALESCE(f.caption, ''::text) || COALESCE('\'::text || e.caption::text, ''::text) AS caption,
            e.parent,
            e.name,
            (COALESCE('\'::text || f.visible, ''::text) || COALESCE('\'::text || e.visible::text, ''::text)) || '\'::text AS visible
           FROM axpages e
             LEFT JOIN (SELECT (COALESCE(d.caption, ''::character varying)::text || '\'::text) || COALESCE(c.caption, ''::character varying)::text AS caption,
                    c.name,
                    (COALESCE('\'::text || d.visible, ''::text) || COALESCE('\'::text || c.visible::text, ''::text)) || '\'::text AS visible
                   FROM axpages c
                     LEFT JOIN(SELECT a.name,
                            a.parent,
                            a.caption,
                            a.levelno,
                            a.ordno,
                            1 AS levlno,
                            ('\'::text || a.visible::text) || '\'::text AS visible
                           FROM axpages a
                          WHERE a.levelno = 0::numeric
                          ORDER BY a.levelno, a.ordno) d ON c.parent::text = d.name::text
                  WHERE c.levelno = ANY(ARRAY[1::numeric, 0::numeric])) f ON e.parent::text = f.name::text
          WHERE e.levelno = ANY(ARRAY[1::numeric, 0::numeric, 2::numeric])) h ON g.parent::text = h.name::text
  WHERE g.levelno <= 3::numeric and g.blobno= 1 and g.visible= 'T' and g.pagetype IS NOT null and g.pagetype <> ''
  ORDER BY g.ordno, g.levelno";

        public const string ARMGETMENUQUERY_ORACLE = @"SELECT REPLACE(REPLACE(COALESCE(h.caption, '') || COALESCE('\' || g.caption, ''), '\\\', '\'), '\\', '\') AS menupath,
       g.name,
       g.caption,
       g.icon,
       g.pagetype,
       g.img,
       g.type,
       g.parent,
       g.visible,
       g.levelno,
       g.updatedby,
       g.createdon,
       g.importedon,
       g.createdby,
       g.updatedby,
       g.category
  FROM axpages g
  LEFT JOIN (SELECT COALESCE(f.caption, '') || COALESCE('\' || e.caption, '') AS caption,
                    e.parent,
                    e.name,
                    (COALESCE('\' || f.visible, '') || COALESCE('\' || e.visible, '')) || '\' AS visible
               FROM axpages e
               LEFT JOIN (SELECT (COALESCE(d.caption, '') || '\') || COALESCE(c.caption, '') AS caption,
                                 c.name,
                                 (COALESCE('\' || d.visible, '') || COALESCE('\' || c.visible, '')) || '\' AS visible
                            FROM axpages c
                            LEFT JOIN (SELECT a.name,
                                              a.parent,
                                              a.caption,
                                              a.levelno,
                                              a.ordno,
                                              1 AS levlno,
                                              ('\' || a.visible) || '\' AS visible
                                         FROM axpages a
                                        WHERE a.levelno = 0
                                        ORDER BY a.levelno, a.ordno) d ON c.parent = d.name
                           WHERE c.levelno IN (0, 1)) f ON e.parent = f.name
              WHERE e.levelno IN (0, 1, 2)) h ON g.parent = h.name
WHERE g.levelno <= 3
   AND g.blobno = 1
   AND g.visible = 'T'
   AND g.pagetype IS NOT NULL
  ORDER BY g.ordno, g.levelno;";

        public const string GET_HOMEPAGECARDS_ORACLE = "SELECT DISTINCT a.axhomeconfigid AS cardid, a.caption, a.pagecaption, a.displayicon, a.stransid, a.datasource, a.moreoption, a.colorcode, a.disporder FROM axhomeconfig a, axuseraccess b, axuserlevelgroups a2 WHERE CASE WHEN (a.pagetype = 'Form' OR a.pagetype = 'Report') THEN SUBSTR(a.stransid, 2) ELSE a.stransid END = b.sname AND (b.rname = a2.usergroup OR a2.usergroup = 'default') AND a2.username = @username  AND CURRENT_DATE BETWEEN a2.startdate AND NVL(a2.enddate, CURRENT_DATE) AND a.active = 'T' ORDER BY a.disporder ";

        public const string GETMENUUSERACCESS = $"select name, caption, type, icon, pagetype, props, img, levelno, intview, pagetype from axpages where blobno = 1 and visible = 'T' and pagetype is not null and pagetype <> '' and name in (select SNAME from AXUSERACCESS a, axusergroups b where (a.rname = b.userroles or b.userroles = 'default') and b.groupname in ($allRole$) and stype = 'p') order by ordno, levelno;";
        public const string GETMENUUSERACCESS_ORACLE = $"select name, caption, type, icon, pagetype, props, img, levelno, intview, pagetype from axpages where blobno = 1 and visible = 'T' and nvl(pagetype,'NA') <> 'NA' and name in (select SNAME from AXUSERACCESS a, axusergroups b where (a.rname = b.userroles or b.userroles = 'default') and b.groupname in ($allRole$) and stype = 'p') order by ordno, levelno;";

        public const string GETMENUFOROTHERROLE_V2 = $"select name,caption,type,visible,img,parent,levelno,pagetype,intview,icon from axpages where blobno=1 and lower(coalesce(webenable,'t')) ='t' and lower(coalesce(pagetype,'s')) <> 'stem' and name in (SELECT SNAME FROM AXUSERACCESS WHERE RNAME IN (select distinct rname from axusergroups a,axuseraccess b where a.userroles  = b.rname and a.groupname in ($allRole$) and stype = 'p') and stype = 'p' ) order by ordno ,levelno";

        public const string GETMENUFORDEFAULTROLE_V2 = "SELECT name, caption, type, visible, img, parent, levelno, pagetype, intview, icon FROM axpages WHERE blobno = 1 AND LOWER(COALESCE(webenable, 't')) = 't' AND LOWER(COALESCE(pagetype, 's')) <> 'stem' ORDER BY ordno, levelno";

        public const string AXPAGES_ORACLE = $"SELECT replace(replace(NVL(h.caption, '' ) || NVL(g.caption , '' ), '\\\' , '\' ), '\\' , '\' ) AS menupath, g.* FROM axpages g LEFT JOIN (SELECT NVL(f.caption, '' ) || NVL(e.caption , '' ) AS caption, e.parent, e.name, NVL(f.visible, '') || NVL(e.visible , '' ) AS visible FROM axpages e LEFT JOIN (SELECT NVL(d.caption, '') || NVL(c.caption, '') AS caption, c.name, NVL(d.visible, '') || NVL(c.visible , '') AS visible FROM axpages c LEFT JOIN (SELECT a.name, a.parent, a.caption, a.levelno, a.ordno, 1 AS levlno, a.visible AS visible FROM axpages a WHERE a.levelno = 0 ORDER BY a.levelno, a.ordno) d ON c.parent = d.name WHERE c.levelno IN (1, 0)) f ON e.parent = f.name WHERE e.levelno IN (1, 0, 2)) h ON g.parent = h.name WHERE g.levelno <= 3 AND g.blobno = 1 AND g.visible = 'T' AND g.pagetype IS NOT NULL AND g.name IN ($names$) ORDER BY g.ordno, g.levelno;";

        public const string AXPAGES = @"SELECT replace(replace(COALESCE(h.caption, '' ) || COALESCE('\'  || g.caption , '' ), '\\\' , '\' ), '\\' , '\' ) AS menupath,g.* FROM axpages g
     LEFT JOIN (SELECT COALESCE(f.caption, '' ) || COALESCE('\'  || e.caption , '' ) AS caption,
            e.parent,
            e.name,
            (COALESCE('\'  || f.visible, '' ) || COALESCE('\'  || e.visible , '' )) || '\'  AS visible
           FROM axpages e
             LEFT JOIN (SELECT (COALESCE(d.caption, ''::character varying)  || '\' ) || COALESCE(c.caption, ''::character varying)  AS caption,
                    c.name,
                    (COALESCE('\'  || d.visible, '' ) || COALESCE('\'  || c.visible , '' )) || '\'  AS visible
                   FROM axpages c
                     LEFT JOIN(SELECT a.name,
                            a.parent,
                            a.caption,
                            a.levelno,
                            a.ordno,
                            1 AS levlno,
                            ('\'  || a.visible ) || '\'  AS visible
                           FROM axpages a
                          WHERE a.levelno = 0::numeric
                          ORDER BY a.levelno, a.ordno) d ON c.parent  = d.name 
                  WHERE c.levelno = ANY(ARRAY[1::numeric, 0::numeric])) f ON e.parent  = f.name 
          WHERE e.levelno = ANY(ARRAY[1::numeric, 0::numeric, 2::numeric])) h ON g.parent  = h.name 
  WHERE g.levelno <= 3::numeric and g.blobno= 1 and g.visible= 'T' and g.pagetype IS NOT null
  and g.name in ($names$)
  ORDER BY g.ordno, g.levelno";

        //public const string AXPAGES = $"select name,caption,type, icon,pagetype,props,img,levelno ,intview from axpages where blobno = 1 and visible= 'T' and pagetype is not null and name in ($names$) order by ordno ,levelno";
        public const string AXACTIVETASKS = $"select * from vw_pegv2_activetasks where lower(touser) = @username order by edatetime desc"; 
        public const string GET_BULKACTIVETASKS = $"select * from vw_pegv2_activetasks where lower(touser) = @username and lower(tasktype) = @tasktype and lower(processname) = @processname and rectype='PEG' order by edatetime desc";
        public const string GETACTIVETASKS = $"select a.taskid, a.taskname, a.tasktype, a.processname, a.transid, a.keyfield, a.keyvalue, a.touser, a.allowsend, a.allowsendflg, a.sendtoactor, a.initiator , a.initiator_approval, p.amendment, aa.recordid from axactivetasks a join axpdef_peg_processmaster p on a.processname=p.caption LEFT JOIN axactivetasks aa ON a.processname  = aa.processname  AND a.keyvalue  = aa.keyvalue  AND a.transid  = aa.transid  AND aa.tasktype  = 'Make'  AND aa.recordid IS NOT NULL where a.taskid = @taskid and lower(a.tasktype) = @tasktype and lower(a.touser) = @username and not exists(select taskid from axactivetaskstatus b where a.taskid=b.taskid)";
        public const string GET_TASKSTATUS = $"select a.taskstatus, a.username,to_char(to_timestamp(a.eventdatetime , 'YYYYMMDDHH24MISSSSS' ), 'dd/mm/yyy hh24:mi:ss' ) AS eventdatetime from axactivetaskstatus a where a.taskid = @taskid";
        public const string GET_TASKSTATUS_ORACLE = $"select a.taskstatus, a.username,to_char(to_timestamp(SUBSTR(a.eventdatetime,1,14), 'YYYYMMDDHH24MISS'), 'dd/mm/yyyy hh24:mi:ss') AS eventdatetime from axactivetaskstatus a where a.taskid = @taskid";
        public const string GETPROCESSTASK = $"select a.touser, a.processname, a.taskname, a.taskid, a.tasktype, a.eventdatetime AS edatetime, to_char(to_timestamp(a.eventdatetime , 'YYYYMMDDHH24MISSSSS' ), 'dd/mm/yyy hh24:mi:ss' ) AS eventdatetime, a.fromuser, a.fromrole, a.displayicon, a.displaytitle, a.displaymcontent, a.displaycontent, a.displaybuttons, a.keyfield, a.keyvalue, a.transid, a.priorindex, a.indexno, a.approvereasons, a.defapptext, a.returnreasons, a.defrettext, a.rejectreasons, a.defregtext, (select distinct b.recordid from axactivetasks b where b.processname  = a.processname  AND b.keyvalue  = a.keyvalue  AND b.transid  = a.transid  AND b.tasktype  = 'Make'  AND b.recordid IS NOT NULL), a.approvalcomments, a.rejectcomments, a.returncomments, a.returnable, a2.taskstatus, a2.statusreason, a2.statustext, a2.username, '' ispending,a.initiator, a.initiator_approval, a.displaysubtitle, a.allowsend, a.allowsendflg, b.cmsg_appcheck, b.cmsg_return, b.cmsg_reject,b.showbuttons from axactivetasks a JOIN axprocessdefv2 b ON a.processname  = b.processname  AND a.taskname  = b.taskname  left join axactivetaskstatus a2 on a.taskid = a2.taskid where a.grouped = 'T' and a.taskid = @taskid";
        public const string GETPROCESSTASK_ORACLE = $"select a.touser, a.processname, a.taskname, a.taskid, a.tasktype, to_char(to_timestamp(SUBSTR(a.eventdatetime,1,14), 'YYYYMMDDHH24MISS'), 'dd/mm/yyyy hh24:mi:ss') AS eventdatetime, a.fromuser, a.fromrole, a.displayicon, a.displaytitle, a.displaymcontent, a.displaycontent, a.displaybuttons, a.keyfield, a.keyvalue, a.transid, a.priorindex, a.indexno, a.approvereasons, a.defapptext, a.returnreasons, a.defrettext, a.rejectreasons, a.defregtext, (select distinct b.recordid from axactivetasks b where b.processname  = a.processname  AND b.keyvalue  = a.keyvalue  AND b.transid  = a.transid  AND b.tasktype  = 'Make'  AND b.recordid IS NOT NULL), a.approvalcomments, a.rejectcomments, a.returncomments, a.returnable, a2.taskstatus, a2.statusreason, a2.statustext, a2.username, '' ispending,a.initiator, a.initiator_approval, a.displaysubtitle, a.allowsend, a.allowsendflg, b.cmsg_appcheck, b.cmsg_return, b.cmsg_reject,b.showbuttons from axactivetasks a JOIN axprocessdefv2 b ON a.processname  = b.processname  AND a.taskname  = b.taskname  left join axactivetaskstatus a2 on a.taskid = a2.taskid where a.grouped = 'T' and a.taskid = @taskid";
        public const string GETPROCESSKEYVALUE = $"select distinct keyvalue from axactivetasks a where lower(a.processname) = @processname";
        public const string GETKEYVALUE = $"select keyvalue from axactivetasks a where recordid = @recordid and lower(a.processname) = @processname";
        public const string GETTSTRUCTSQLQUERY = $"select fldsql  from axpflds a where modeofentry in('accept','select') and lower(tstruct) = @transid and lower(fname) = @field";
        public const string GETPOWERUSER = "select username,password,active,isfirsttime from axusers where lower(username) = @username and password = @password and active = 'T'";
        public const string GETPOWERUSERROLES = "select distinct groupname as USERROLES from axusergroups where groupname in (Select distinct usergroup from AXUSERLEVELGROUPS WHERE lower(USERNAME) = @username) order by groupname";
        public const string VALIDATEPOWERUSERS = "select username,password,active from axusers where lower(username) = @username   and active = 'T'";
        public const string VALIDATEPOWERUSERSWITHPASSWORD = "select username,password,active from axusers where lower(username) = @username and password = @password and active = 'T'";
        public const string DISNTICTROLES = "select distinct groupname as USERROLES  from axusergroups";
        public const string SELECTFROMAXMOBILENOTIFY = "select * from ax_mobilenotify where lower(username) = @username and lower(projectname) = @appname";
        public const string UPDATEAXMOBILENOTIFY = $"update ax_mobilenotify SET GUID='" + "$guid$" + "',FIREBASE_ID='" + "$firebaseId$" + "',IMEI_NO='" + "$ImeiNo$" + "',STATUS='" + ("$status$" == "true" ? "t" : "f") + "' where username='" + "$userName$" + "' and projectname='" + "$appName$" + "'";
        public const string INSERTINTOAXMOBILENOTIFY = $"insert into ax_mobilenotify (USERNAME,PROJECTNAME,GUID,FIREBASE_ID,IMEI_NO,STATUS) values ('" + "$userName$" + "','" + "$appName$" + "','" + "$notificationguid$" + "','" + "$notificationfirebaseId$" + "','" + "$notificationImeiNo$" + "','" + ("$notificationstatus$" == "true" ? "t" : "f") + "')";
        public const string ARMGetPageData = $"select * FROM  public.\"$formNamesPageDataTable$\" WHERE \"formname\"  IN($allInlineform$) and   \"keyvalue\" = '$Keyvalue$'";
        public const string ARMGetAXPROCESS = $"select eventdatetime , taskid , transid ,processname , taskname , keyvalue ,taskstatus from axprocess where lower(processname) = @processname";
        public const string ARMPROCESSDEFINITION = "select a.*, indexno + 1 as nextindexno from vw_pegv2_processdef_tree a where lower(processname) = @processname  order by indexno";
        public const string ARMGETPROCESSLIST = "select 'completed' completionstatus, taskname,tasktype,tasktime,taskfromuser FromUser, taskstatus,displayicon,displaytitle,taskid,keyfield,keyvalue,recordid,transid  from pr_pegv2_processlist( @processname) where lower(taskfromuser) = @username union all  select 'pending' completionstatus, TaskName,TaskType,eventdatetime tasktime,FromUser,'',DisplayIcon,DisplayTitle,taskid,keyfield,keyvalue,recordid,transid from vw_pegv2_activetasks where lower(touser) = @username and lower(processname) = @processname order by 4 desc";
        public const string ARMGETPROCESSDETAIL = "select * from pr_pegv2_processprogress(@processname,@keyvalue) where rnum=1";
        public const string ARMGETPROCESSDETAIL_ORACLE = "select pr_pegv2_processprogress(@processname,@keyvalue) FROM dual";
        
        public const string UPDATEPOWERUSERPASSWORD = "UPDATE axusers SET password = @newpassword, ppassword = @oldpassword,isfirsttime='F' WHERE username = @username AND password = @oldpassword";
        public const string GETFORGETPASSWORDPOWERUSER = "select email,username from axusers where username=@username and email=@email";
        public const string UPDATEFORGOTPASSWORD = "UPDATE axusers SET password = @MD5OTP ,isfirsttime='T' WHERE username = @username";
       
        public const string ARMGETADDNEWNODE = "select displayicon taskicon, taskname, transid from vw_pegv2_processdef_tree where lower(processname) = @processname and tasktype = 'Make' and groupwithprior = 'F' order by indexno";
        public const string GETUSERPASSWORD = "select password from axusers where lower(username) = @username";
        public const string GETDATASOURCESSQL = "SELECT sqltext  FROM Axdirectsql where lower(sqlname) = @datasource";
        public const string GETACTIVETASKPARAMS = "select taskparams from  axactivetaskparams a where lower(transid) = @transid and lower(keyvalue) = @keyvalue and lower(taskstatus) = 'made' order by eventdatetime desc";
        public const string GET_TIMELINE_DATA = "select * from pr_pegv2_processlist(@processname) where lower(keyvalue)= @keyvalue   order by tasktime desc";
        public const string GET_TIMELINE_DATA_ORACLE = "select pr_pegv2_processlist(@processname, @keyvalue) from dual"; //Issue
        //select pr_pegv2_processlist('QAProcess') FROM dual


        public const string GETCARDLISTS = "SELECT axp_cardsid, cardname, cardtype, charttype, chartjson, cardicon, pagename, pagedesc, cardbgclr, width, height, cachedata, autorefresh, sql_editor_cardsql AS cardsql, orderno, accessstring, htransid, htype, hcaption, axpfile_imgcard, html_editor_card, calendarstransid FROM axp_cards WHERE $cardsFilter$ ORDER BY orderno";
        public const string GETCARDlISTS_ORACLE = ""; 
        //public const string GET_PROCESSCARDTASKPARAMS = "select taskparams from axactivetaskparams a where processname = @processname and taskname = @taskname and keyvalue = @keyvalue";
        public const string GET_PROCESSCARDS = "select a.*, a.cardsid as  cardid from vw_pegv2_global_cards a where lower(processname) = @processname and lower(taskname) = @taskname ";
        public const string GET_PROCESSUSERTYPE = "select distinct tasktype from axactivetasks where lower(processname) = @processname and lower(touser) = @username";
        public const string GET_SENDTOUSERS = "select * from pr_pegv2_sendto_userslist(@allowsendflg,@actor,@processname,@keyvalue, @taskname)";
        public const string GET_SENDTOUSERS_ORACLE = "select pr_pegv2_sendto_userslist(@allowsendflg,@actor,@processname,@keyvalue, @taskname) users from dual";

        public const string GETGEOFENCINGDATA = "select * from vw_geoloca_config where employee_code= @username";
        public const string GET_NEXTTASKINPROCESS = "select taskid,keyfield,keyvalue,nexttasktype from (select RANK() OVER(order by edatetime desc) rnk,vpa.taskid,vpa.keyfield,vpa.keyvalue, 'Current Process' nexttasktype from vw_pegv2_activetasks vpa where lower(touser) = @username and lower(processname) = @processname and lower(keyvalue) = @keyvalue union select RANK() OVER(order by edatetime desc) rnk,vpa.taskid,vpa.keyfield,vpa.keyvalue, 'Pending Process' from vw_pegv2_activetasks vpa where lower(touser) = @username and lower(processname) = @processname ) a where rnk=1 order by nexttasktype";
        public const string GET_EDITABLETASK = "SELECT fn_pegv2_editabletask(@processname,@taskname,@keyvalue,@username,@indexno) editable from dual";
        public const string GET_OPTIONALTASK = "select distinct a.isoptional isoptional from axactivetasks a where a.taskid = @taskid";
        //public const string GET_HOMEPAGECARDS = "select distinct a.axhomeconfigid cardid,a.caption,a.pagecaption,a.displayicon,a.stransid,a.datasource,a.moreoption,a.colorcode,a.disporder  from axhomeconfig a, axuseraccess b, axuserlevelgroups a2 where case when (pagetype='Form' or  pagetype='Report') then substring(a.stransid,2) else a.stransid end = b.sname and (b.rname = a2.usergroup or a2.usergroup = 'default') and a2.username = @username and current_date between a2.startdate  and coalesce(a2.enddate, current_date) and a.active = 'T' order by a.disporder ";
        public const string GET_HOMEPAGECARDS = "select axhomeconfigid cardid,caption,pagecaption,displayicon,stransid,datasource,moreoption,colorcode,groupfolder,grppageid from axhomeconfig where active = 'T' order by disporder asc";
        public const string HOMEPAGECARDSV2 = "select axhomeconfigid cardid,caption,pgname,displayicon,stransid,datasource,colorcode,groupfolder,grppageid,carddesc,cardhide,html_editor_card,paneltypecnd,paneltype,moreoption from axhomeconfig where cardhide!='T' order by disporder asc";
        public const string GET_NEXTOPTIONALTASK = @"select distinct a.*  from pr_pegv2_processprogress(@processname,  @keyvalue) a join axactivetasks b on a.taskid = b.taskid where a.rnum=1 and a.taskstatus = 'Active'and b.isoptional  = 'T' order by indexno desc";
        //GET_NEXTOPTIONALTASK - TODO
        
        #region Active List - Home Page
        public const string GET_ACTIVETASKSLIST = $"select * from vw_pegv2_activetasks where lower(touser) = @username order by edatetime desc LIMIT @pagesize OFFSET @offset";
        public const string GET_ACTIVETASKSLIST_ORACLE = $"SELECT * FROM ( SELECT * FROM vw_pegv2_activetasks  WHERE LOWER(touser) = @username ORDER BY edatetime DESC )  WHERE ROWNUM between @startrow and @endrow";
        public const string GET_COMPLETEDTASKSLIST = @"select * from vw_pegv2_completed_tasks  where lower(username) = @username order by edatetime desc LIMIT @pagesize OFFSET @offset";
        public const string GET_COMPLETEDTASKSLIST_ORACLE = $"SELECT * FROM ( select * from vw_pegv2_completed_tasks  where lower(username) = @username order by edatetime desc)  WHERE ROWNUM between @startrow and @endrow";
        public const string GET_ACTIVETASKSCOUNT = $"select count(*) count from vw_pegv2_activetasks where lower(touser) = @username";
        public const string GET_COMPLETEDTASKSCOUNT = @"select count(*) count from vw_pegv2_completed_tasks where lower(username) = @username";
        public const string GET_FILTEREDACTIVETASKSLIST = $"select * from vw_pegv2_activetasks where lower(touser) = @username $DATEFILTER$ $FROMUSERFILER$ $PROCESSFILER$ $SEARCHFILTER$ order by edatetime desc LIMIT @pagesize OFFSET @offset";
        public const string GET_FILTEREDCOMPLETEDTASKSLIST = @"select * from vw_pegv2_completed_tasks  where lower(username) = @username $DATEFILTER$ $FROMUSERFILER$ $PROCESSFILER$ $SEARCHFILTER$ order by edatetime desc LIMIT @pagesize OFFSET @offset";
        public const string GET_BULKAPPROVALCOUNT = @"select processname, count(*) pendingapprovals from vw_pegv2_activetasks where tasktype = 'Approve' and rectype='PEG' and lower(touser) = @username
        group by processname order by processname asc";
        #endregion

        #region Publish API
        public const string GET_PUBLISHEDAPI = "select * from axpdef_publishapi where publickey = @publickey";
        #endregion
        #region Inbound Queue
        public const string GET_INBOUNDQUEUE = "select axqueuename,uname,secretkey from AxInQueues where axqueuename = @queuename and active = 'T'";
        #endregion

        #region GeoFencing 
        public const string GET_USERGEOLOCATIONCONFIG = "select USERNAME, FIREBASEID, EXPECTEDLOCATIONS,ACTIVE,INTERVAL from vw_axlocationtrackingconfig where lower(username) = @username";
        public const string INSERT_USERGEOLOCATIONDATA = "INSERT INTO AxGeoFencingData (USERNAME , CURRENT_NAME , CURRENT_LOC , EXPECTEDLOCATIONS,LOCATION_ARRAY,IDENTIFIER) VALUES (@username,  @current_name,  @current_loc,  @expectedlocations,@location_array,@identifier)";
        public const string GET_FIREBASEID = "SELECT FIREBASE_ID FIREBASEID FROM ax_mobilenotify where lower(username) = @username";
        #endregion



        #region Mobile Notify
        public const string SELECT_AXMOBILENOTIFY = "select * from ax_mobilenotify where (lower(username) = lower(@username) or lower(firebase_id) = lower(@firebaseid) ) and lower(projectname) = @appname";
        public const string DELETE_AXMOBILENOTIFY = "delete from ax_mobilenotify where  (lower(username) = lower(@username) or lower(firebase_id) = lower(@firebaseid) ) and lower(projectname) = @appname";
        public const string DELETE_AXMOBILENOTIFYFORUSER = "delete from ax_mobilenotify where  lower(username) = lower(@username) and lower(projectname) = @appname";
        //public const string UPDATEAXMOBILENOTIFY = $"update ax_mobilenotify SET GUID='" + "$guid$" + "',FIREBASE_ID='" + "$firebaseId$" + "',IMEI_NO='" + "$ImeiNo$" + "',STATUS='" + ("$status$" == "true" ? "t" : "f") + "' where firebase_id='" + "$firebaseId$" + "' and projectname='" + "$appName$" + "'";
        public const string INSERT_AXMOBILENOTIFY = $"insert into ax_mobilenotify (USERNAME,PROJECTNAME,GUID,FIREBASE_ID,IMEI_NO,STATUS) values ('" + "$userName$" + "','" + "$appName$" + "','" + "$notificationguid$" + "','" + "$notificationfirebaseId$" + "','" + "$notificationImeiNo$" + "','" + ("$notificationstatus$" == "true" ? "t" : "f") + "')";
        #endregion


        #region Entity Listing
        public const string GET_ENTITYFORMMETADATA =
            @"select dname fname,caption,asgrid customdatatype, 'dc' datatype, '' hidden, dname dcname  from axpdc where tstruct =  @transid
            union all 
            select fname,caption,customdatatype,datatype,hidden, dcname dcname  from axpflds where tstruct =  @transid
            union all 
            select script,title,'Button',null, case (visible) when 'true' then 'F' else 'T' end hidden , parentdc dcname from axtoolbar a where stype='tstruct' and name = @transid and script is not null";

        public const string GET_ENTITYMETADATA_ANALYTICS = @"select * from fn_axpanalytics_metadata(@transid, 'F')";

        public const string GET_ENTITYDATA_ANALYTICS = @"SELECT * from fn_axpanalytics_listdata(@transid, @fields, @pagesize, @pageno, 'NA', @username, 'T')";
        public const string GET_FILTERED_ENTITYDATA_ANALYTICS = @"SELECT * from fn_axpanalytics_listdata(@transid, @fields, @pagesize, @pageno, @filter, @username, 'T')";

        public const string GET_ENTITYCHARTSDATA_GENERAL_ANALYTICS = @"SELECT * from fn_axpanalytics_chartdata('Entity', @transid, 'General', @criteria, 'NA', @username, 'T')";
        public const string GET_ENTITYCHARTSDATA_CUSTOM_ANALYTICS = @"SELECT unnest(string_to_array(fn_axpanalytics_chartdata,'^^^')) from fn_axpanalytics_chartdata('Entity', @transid, @condition, @criteria, 'NA', @username, 'T')";

        public const string GET_ANALYTICSPAGECHARTSDATA_CUSTOM_ANALYTICS = @"SELECT fn_axpanalytics_ap_charts(@transid, @criteria, 'NA', @username, 'T')";

        public const string GET_FILTER_FIELDDATA_ANALYTICS = @"select * from fn_axpanalytics_filterdata(@transid, @filter) order by 1";

        public const string GET_SUBENTITYMETADATA_ANALYTICS = @"select * from fn_axpanalytics_metadata(@transid, 'T')";
        public const string GET_SUBENTITYDATA_ANALYTICS = @"select  unnest(string_to_array(fn_axpanalytics_se_listdata,'^^^'))  from fn_axpanalytics_se_listdata(@transid, @fields, @pagesize, @pageno)";
        public const string GET_SUBENTITYCHARTSDATA_ANALYTICS = @"SELECT unnest(string_to_array(fn_axpanalytics_chartdata,'^^^'))  from fn_axpanalytics_chartdata('Subentity', @transid, @condition, @criteria, 'NA', @username, 'T')";

        public const string GET_ENTITYMETADATA_ANALYTICS_ORACLE = @"select * from table(fn_axpanalytics_metadata(@transid, 'F')) ORDER BY ftransid,griddc";
        public const string GET_ENTITYDATA_ANALYTICS_ORACLE = @"SELECT * FROM TABLE(fn_axpanalytics_listdata(@transid, @fields, @pagesize, @pageno))";
        public const string GET_FILTERED_ENTITYDATA_ANALYTICS_ORACLE = @"SELECT * FROM TABLE(fn_axpanalytics_listdata(@transid, @fields, @pagesize, @pageno, @filter))";
        public const string GET_ENTITYCHARTSDATA_ANALYTICS_ORACLE = @"SELECT * from TABLE(fn_axpanalytics_chartdata('Entity', @transid, @condition, @criteria))";
        public const string GET_FILTER_FIELDDATA_ANALYTICS_ORACLE = @"select * from table(fn_axpanalytics_filterdata((@transid, @filter)) order by 1";

        public const string GET_SUBENTITYMETADATA_ANALYTICS_ORACLE = @"select * from table(fn_axpanalytics_metadata(@transid, 'T'))";
        public const string GET_SUBENTITYDATA_ANALYTICS_ORACLE = @"SELECT * FROM TABLE(fn_axpanalytics_se_listdata(@transid, @fields, @pagesize, @pageno))";
        public const string GET_SUBENTITYCHARTSDATA_ANALYTICS_ORACLE = @"SELECT * from TABLE(fn_axpanalytics_chartdata('Subentity', @transid, @condition, @criteria))";

        public const string GET_ALLENTITYLIST_ANALYTICS_DEFAULTROLE = @"SELECT t.name, t.caption FROM tstructs t inner join axpages p on p.pagetype = ('t' || t.name) and  t.blobno = 1  and p.blobno=1 and lower(coalesce(p.webenable,'t')) ='t' and lower(coalesce(p.pagetype,'s')) <> 'stem'  order by trim(t.caption) asc";
        public const string GET_ALLENTITYLIST_ANALYTICS_OTHERROLES = @"SELECT t.name, t.caption FROM tstructs t inner join axpages p on p.pagetype = ('t' || t.name) and  t.blobno = 1  and p.blobno=1 and lower(coalesce(p.webenable,'t')) ='t' and lower(coalesce(p.pagetype,'s')) <> 'stem' and p.name in (SELECT SNAME FROM AXUSERACCESS WHERE RNAME IN (select distinct rname from axusergroups a,axuseraccess b where a.userroles  = b.rname and a.groupname in ( $OTHERROLES$ ) and stype = 'p') and stype = 'p' ) order by trim(t.caption) asc";
        public const string GET_FILTERED_ENTITYLIST_ANALYTICS_DEFAULTROLE = @"SELECT t.name, t.caption FROM tstructs t inner join axpages p on p.pagetype = ('t' || t.name) and  t.blobno = 1  and t.name in ( $ENTITYLIST$ ) and p.blobno=1 and lower(coalesce(p.webenable,'t')) ='t' and lower(coalesce(p.pagetype,'s')) <> 'stem'";
        public const string GET_FILTERED_ENTITYLIST_ANALYTICS_OTHERROLES = @"SELECT t.name, t.caption FROM tstructs t inner join axpages p on p.pagetype = ('t' || t.name) and  t.blobno = 1 and t.name in ( $ENTITYLIST$ ) and p.blobno=1 and lower(coalesce(p.webenable,'t')) ='t' and lower(coalesce(p.pagetype,'s')) <> 'stem' and p.name in (SELECT SNAME FROM AXUSERACCESS WHERE RNAME IN (select distinct rname from axusergroups a,axuseraccess b where a.userroles  = b.rname and a.groupname in ( $OTHERROLES$ ) and stype = 'p') and stype = 'p' )";

        #endregion


        #region payment integration
        public const string INSERTREQUESTSTRING = "INSERT INTO axrequest(requestid, requestreceivedtime, sourcefrom, requeststring, headers, params, authz, contenttype, contentlength, host, url, endpoint, requestmethod, username, additionaldetails, sourcemachineip, apiname) VALUES('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', '{16}')";
        public const string INSERTRESPONSESTRING = "INSERT INTO axresponse(responseid, responsesenttime, statuscode, responsestring, headers, contenttype, contentlength, errordetails, endpoint, requestmethod, username, additionaldetails, requestid, executiontime) VALUES ('{0}', '{1}', '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}')";
        public const string INSERTRESPONSESTRING_ORACLE = "INSERT INTO AXRESPONSE (RESPONSEID, RESPONSESENTTIME, STATUSCODE, HEADERS, CONTENTTYPE, CONTENTLENGTH, ERRORDETAILS, ENDPOINT, REQUESTMETHOD, USERNAME, ADDITIONALDETAILS, REQUESTID, EXECUTIONTIME, RESPONSESTRING) VALUES ('{0}', TO_TIMESTAMP('{1}', 'YYYY-MM-DD HH24:MI:SS.FF'), {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', @responsestr )";
        public const string INSERTREQUESTSTRING_ORACLE = "INSERT INTO AXREQUEST (REQUESTID, REQUESTRECEIVEDTIME, SOURCEFROM, HEADERS, PARAMS, AUTHZ, CONTENTTYPE, CONTENTLENGTH, HOST, URL, ENDPOINT, REQUESTMETHOD, USERNAME, ADDITIONALDETAILS, SOURCEMACHINEIP, APINAME, REQUESTSTRING) VALUES ('{0}', TO_TIMESTAMP('{1}', 'YYYY-MM-DD HH24:MI:SS.FF'), '{2}', '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', '{12}', '{13}', '{14}', '{15}', @requeststr ) ";
        public const string PAYMENTSQL = "SELECT f_get_api_json(@masterid)";
        public const string DOCUMENTUPLOADSQL = "select sourceid recordid,f_get_api_json_document(@recordId),axpattachmentpath from outboundhdr o where o.outboundhdrid = @recordId";
        public const string GetFormDetails = "select * from axpflds a where a.tstruct = @formname";
        public const string GetColumnDefination = "SELECT COLUMN_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @tablename";

        //public const string DOCUMENTUPLOADSQL = "select f_get_api_json_document(1223010000020),'C:\\Users\\preeti.b\\Downloads' as axpattachmentpath,'1967550000006' as recordid";
        //select f_get_api_json_document(1223010000020),'C:\\Users\\preeti.b\\Downloads' as axpattachmentpath,'1937660000090' as recordid


        #endregion
        #region Axpert Jobs Scheduler
        public const string GET_AXPERTJOBSDETAILS = "select  rediskeyname, rediskeyval,jobid, TO_CHAR(modifiedon, 'yyyyMMddHHmmss') AS modifiedon,isactive  from  AXPDEF_JOBS a where lower(a.jname) = @jobname";
        #endregion

        #region Periodic Notifications 
        public const string GET_PERIODICNOTIFICATIION_MODIFIEDON = "select TO_CHAR(modifiedon, 'yyyyMMddHHmmss') AS modifiedon,active,name,axperiodnotifyid from  AXPERIODNOTIFY a where a.name = @name";
        #endregion

        #region API Service
        public const string INSERT_TO_APILOG = "INSERT INTO axapijobdetails(jobid, requestid, url, method, requeststr, params, header, responsestr, status, starttime, endtime, context, servicename) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}')";
        #endregion

        #region Queue Exchange
        public const string GET_QUEUEDETAILS = "select username,transid,keyfield,target,queuename,secretkey,armurl from fn_queue_details(@transid,@clientcode)";
        #endregion

        public const string INSERT_TO_AXACTIVEMESG = "INSERT INTO axactivemessages(eventdatetime, msgtype,fromuser,touser,tasktype,processname,taskname,transid, displaytitle,displaycontent,hlink_params) values('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}')";

    }
}
