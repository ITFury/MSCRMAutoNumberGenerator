//CRM Forms common data manager


/*---CONSTANTS---*/

//Form types
var FORM_TYPE_CREATE = 1;
var FORM_TYPE_UPDATE = 2;


/*---FUNCTIONS---*/

function isOdd(num) {
    ///<summary>Validate is passed odd number or not.</summary>
    ///<param name="num" type="number">Number</param>
    ///<return>true or false</return>
    return !!(num % 2);
}


function showModalDialog(errorMsg)
    ///<summary>Show error message in Modal Dialog.</summary>
    ///<param name="errorMsg" type="string">Error message</param>
{
    var dlgTitle = "Error";
    var dlgBody = errorMsg;

    //Passing parameters to web resource
    var addParams = "dlgTitle=" + dlgTitle + "&dlgBody=" + dlgBody;
    var webResourceUrl = "/WebResources/op_ModalDialog?Data=" + encodeURIComponent(addParams);

    var dialogOptions = new Xrm.DialogOptions();
    dialogOptions.width = 350;
    dialogOptions.height = 200;

    //Open Modal Dialog
    Xrm.Internal.openDialog(webResourceUrl, dialogOptions, null, null, null);
}


///OData (Open Data Protocol) is an open Web protocol for querying and updating data. 
///The protocol use HTTP command requests and receive responses in XML or JSON format.
function _getODataEndpointUrl() {
    ///<summary>Get oData Endpoint Url</summary>
    ///<return>oData Endpoint Url</return>

    var serverUrl = document.location.protocol + "//" + document.location.host + "/" + Xrm.Page.context.getOrgUniqueName();

    return serverUrl + "/XRMServices/2011/OrganizationData.svc";
}


function _retrieve(oDataQuery) {
    ///<summary>Retrieve data from server</summary>
    ///<param name="oDataQuery" type="String">query string</param>
    ///<return>data from server</return>

    var result = null;

    var retrieveRecordsReq = new XMLHttpRequest();
    retrieveRecordsReq.open("GET", oDataQuery, false);
    retrieveRecordsReq.setRequestHeader("Accept", "application/json");
    retrieveRecordsReq.setRequestHeader("Content-Type", "application/json; charset=utf-8");

    retrieveRecordsReq.onreadystatechange = function () {

        if (this.readyState == 4 && this.status == 200) {
            var retrievedRecords = JSON.parse(retrieveRecordsReq.responseText).d;

            result = retrievedRecords.results;
        }
    };
    if (typeof (retrieveRecordsReq.send()) != "undefined") {
        retrieveRecordsReq.send();
    }

    return result;
}


function _createODataQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn) {
    ///<summary>Construct oData query</summary>
    ///<param name="entityName" type="String">entity name. Example: Account</param>
    ///<param name="searchFieldNames" type="String">search field name array. Example: {Telephone1,Name}</param>
    ///<param name="searchFieldValues" type="String">search field value array. Example: {32134132,karlis}</param>
    ///<param name="searchOperator" type="String">search field value array. Example: {eq,le}</param>
    ///<param name="fieldsToReturn" type="String">search fields to return. Example: "*" Returns all fields; Example: "Name,Telephone1" Returns name and telephone fields</param>
    ///<param name="recordsToReturn" type="String">records to retrieve, max 50. Example: 5</param>
    ///<return>entity query</return>

    var oDataEndpointUrl = _getODataEndpointUrl();

    var oDataQuery = oDataEndpointUrl + "/" + entityName + "Set?$select=" + fieldsToReturn;

    var filter = "&$filter=";
    for (i = 0; i < searchFieldNames.length; i++) {

        //if searchFieldValues[i] is Number, then remove qoutes, else add qoutes
        var removeQuotes = Number(searchFieldValues[i])
            || searchFieldValues[i] == "0"
            || searchFieldValues[i] == "true"
            || searchFieldValues[i] == "false";

        var searchValue = removeQuotes ? " " + searchFieldValues[i] : "'" + searchFieldValues[i] + "'";

        filter += searchFieldNames[i] + " " + searchOperator[i] + searchValue;
        if (i != (searchFieldNames.length - 1))
            filter += " and ";
    }
    oDataQuery += filter
    if (recordsToReturn != "")
        oDataQuery += "&$top=" + recordsToReturn;

    return oDataQuery;
}


function retrieveEntityByQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn) {
    ///<summary>Construct oData query and retrieve entity data</summary>
    ///<param name="entityName" type="String">entity name. Example: Account</param>
    ///<param name="searchFieldNames" type="String">search field name array. Example: {Telephone1,Name}</param>
    ///<param name="searchFieldValues" type="String">search field value array. Example: {32134132,karlis}</param>
    ///<param name="searchOperator" type="String">search field value array. Example: {eq,le}</param>
    ///<param name="fieldsToReturn" type="String">search fields to return. Example: "*" Returns all fields; Example: "Name,Telephone1" Returns name and telephone fields</param>
    ///<param name="recordsToReturn" type="String">records to retrieve, max 50</param>
    ///<return>entity data</return>

    var oDataQuery = _createODataQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn);

    return _retrieve(oDataQuery);
}


function retrieveEntityByGuid(entityName, searchFieldName, searchFieldGuid, fieldsToReturn) {
    ///<summary>Retrieve entity data by guid</summary>
    ///<param name="entityName" type="String">entity name. Example: account</param>
    ///<param name="searchFieldName" type="String">search field name. Example: accountId</param>
    ///<param name="searchFieldGuid" type="Guid">search field guid. Example: {25892e17-80f6-415f-9c65-7395632f0223}</param>
    ///<param name="fieldsToReturn" type="String">search fields to return. Example: "*" Returns all fields; Example: "Name,Telephone1" Returns name and telephone fields</param>
    ///<return>entity data</return>

    var oDataEndpointUrl = _getODataEndpointUrl();

    var oDataQuery = oDataEndpointUrl + "/" + entityName + "Set?" +
      "$select=" + fieldsToReturn +
      "&$filter=" + searchFieldName + " eq (guid'" + searchFieldGuid + "')" +
      "&$top=1";

    return _retrieve(oDataQuery) != null ? _retrieve(oDataQuery)[0] : null;
}