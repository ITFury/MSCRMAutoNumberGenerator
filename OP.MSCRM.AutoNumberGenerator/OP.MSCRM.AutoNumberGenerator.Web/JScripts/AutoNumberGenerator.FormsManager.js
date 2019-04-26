//CRM Forms common data manager


/*---CONSTANTS---*/

const formTypes = { create: 1, update: 2 };
const orgServiceEndpointPart = "/XRMServices/2011/OrganizationData.svc";


/*---VARIABLES---*/

var formContext;
var isCreateForm;
var isUpdateForm;
var errorMsg;


/*---FUNCTIONS---*/

function formInit(executionContext)
{
    formContext = executionContext.getFormContext();
    if (formContext != null) {
        var formType = formContext.ui.getFormType();
        isCreateForm = formType == formTypes.create;
        isUpdateForm = formType == formTypes.update;
    }
}


if (!String.format) {
    ///<summary>Format message</summary>
    String.format = function (format) {
        var args = Array.prototype.slice.call(arguments, 1);
        return format.replace(/{(\d+)}/g, function (match, number) {
            return typeof args[number] != 'undefined'
                ? args[number]
                : match
                ;
        });
    };
}


function openAlertDialog(msg)
    ///<summary>Displays an alert dialog containing a message and a button.</summary>
    ///<param name="msg" type="string">Message</param>
{
    var alertStrings = { text: msg };
    Xrm.Navigation.openAlertDialog(alertStrings, null);
}


///OData (Open Data Protocol) is an open Web protocol for querying and updating data. 
///The protocol use HTTP command requests and receive responses in XML or JSON format.
function _getODataEndpointUrl() {
    ///<summary>Get oData Endpoint Url</summary>
    ///<return>oData Endpoint Url</return>

    var globalContext = Xrm.Utility.getGlobalContext();
    var clientUrl = globalContext.getClientUrl();
    var endpointUrl = String.format('{0}{1}', clientUrl, orgServiceEndpointPart);

    return endpointUrl;
}


function _retrieve(oDataQuery) {
    ///<summary>Retrieve data from server</summary>
    ///<param name="oDataQuery" type="String">query string</param>
    ///<return>data from server</return>

    var result = null;

    var request = new XMLHttpRequest();
    request.open("GET", oDataQuery, false);
    request.setRequestHeader("Accept", "application/json");
    request.setRequestHeader("Content-Type", "application/json; charset=utf-8");

    request.onreadystatechange = function () {

        if (this.readyState == 4 && this.status == 200) {
            var retrievedRecords = JSON.parse(request.responseText).d;

            result = retrievedRecords.results;
        }
    };

    if (typeof (request.send()) != "undefined") {
        request.send();
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
    oDataQuery += filter;
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