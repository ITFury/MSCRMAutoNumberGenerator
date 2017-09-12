//op_auto_number_config entity Form events
//Related AutoNumberGenerator.FormsManager.js


/*---EVENT HANDLERS---*/

function op_auto_number_configOnLoad()
{
    _setDisabled();
}


function op_auto_number_configOnSave(executionObj) {
    _setUniqueName(executionObj);
}


function op_entity_nameOnChange()
{
    _validatePresentedEntity();

    var executionObj = null;
    _setUniqueName(executionObj);
}


function op_field_nameOnChange()
{
    _validatePresentedEntity();

    var executionObj = null;
    _setUniqueName(executionObj);
}


function op_formatOnChange() {

    _validateFormat();
}


/*---CONSTANTS---*/

//Variable is needed to display Error Message only once. CRM specific: after value is set to null Error Message appears twice.
var ERROR_COUNTER = 0;


/*---FUNCTIONS---*/

function _setDisabled()
    ///<summary>Set field behavior to read-only mode</summary>
{
    var isUpdateForm = Xrm.Page.ui.getFormType() == FORM_TYPE_UPDATE;
   
    //If the form is in Update state set fields to read-only
    if (isUpdateForm)
    {
        Xrm.Page.getControl("op_entity_name").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_field_name").setDisabled(isUpdateForm);
    }
}


function _showError(errorMsg, fieldName)
    ///<summary>Display error message</summary>
    ///<param name="errorMsg" type="String">Error message</param>
    ///<param name="fieldName" type="String">Entity field name that need to clear</param>
{

    //Display error message only if counter is odd number.
    if (ERROR_COUNTER != 0 && isOdd(ERROR_COUNTER))
    {
        showModalDialog(errorMsg);
    }
    //clear field value
    Xrm.Page.getAttribute(fieldName).setValue(null);
    ERROR_COUNTER++;

}


function _validatePresentedEntity()
    ///<summary>Validate presented Entity Name and Entity Field existence. If doesn't exist then display error message.</summary>
    ///<return>Error message</return>
{

    //get op_entity_name field value
    var searchEntityName = Xrm.Page.getAttribute("op_entity_name").getValue();
   
    if (searchEntityName != null)
    {
        var entityName = "SystemForm";
        var searchFieldNames = ["ObjectTypeCode"];
        var searchFieldValues = [searchEntityName];
        var searchOperator = ["eq"];
        var fieldsToReturn = ["FormXml"];
        var recordsToReturn = "";

        var systemForm = retrieveEntityByQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn);

        //Entity with presented Entity Name exist
        if (systemForm != null)
        {
            //validate every Form fields
            for (i = 0; i < systemForm.length; i++)
            {
                var formXml = systemForm[i].FormXml;
                if (formXml != null) {
                    //get op_field_name field value
                    var fieldName = Xrm.Page.getAttribute("op_field_name").getValue();
                    if (fieldName != null) {
                        var fieldExist = formXml.indexOf('"' + fieldName + '"');

                        //Field exist. Don't continue.
                        if (fieldExist != -1) {
                            return;
                        }
                        //Field of presented Entity Form doesn't exist
                        else if (i == systemForm.length - 1 && fieldExist == -1)
                        {
                            //Display error message
                            var errorMsg = "Field '" + fieldName + "' of Entity '" + searchEntityName + "' doesn't exist.";
                            _showError(errorMsg, "op_field_name");
                        }
                    }
                }
            }
        }
        //Entity with presented Entity Name doesn't exist
        else
        {
            //Display error message
            var errorMsg = "Entity '" + searchEntityName + "' doesn't exist.";
            _showError(errorMsg, "op_entity_name");
        }
    }
}


function _uniqueNameDuplicateDetection(executionObj, uniqueName)
    ///<summary>Detect Unique Name duplicate</summary>
    ///<param name="executionObj" type="object">Execution object</param>
    ///<param name="uniqueName" type="String">Unique entity name. Example: account-accountnumber</param>
    ///<return>Error message</return>
{

    var entityName = "op_auto_number_config";
    var searchFieldNames = ["op_unique_name"];
    var searchFieldValues = [uniqueName];
    var searchOperator = ["eq"];
    var fieldsToReturn = ["op_unique_name"];
    var recordsToReturn = "";

    var autoNumberConfig = retrieveEntityByQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn);

    if (autoNumberConfig != null && autoNumberConfig.length > 0)
    {
        //Display error message
        var errorMsg = "Entity Unique Name '" + uniqueName + "' already exist. Please change Entity Name or Entity Field value.";
        showModalDialog(errorMsg);

        if (executionObj != null)
        {
            //Cancel form save
            executionObj.getEventArgs().preventDefault();
        }
    }
}


function _setUniqueName(executionObj) {
    ///<summary>Set record Unique Name in format: <op_entity_name> - <op_field_name></summary>
    ///<param name="executionObj" type="object">Execution object</param>

    var isCreateForm = Xrm.Page.ui.getFormType() == FORM_TYPE_CREATE;
   
    //If the form is in Create state set Unique Name
    if (isCreateForm) {

        if (Xrm.Page.getAttribute("op_entity_name") != null
            && Xrm.Page.getAttribute("op_field_name") != null)
        {
            //get values
            var entityName = Xrm.Page.getAttribute("op_entity_name").getValue();
            var fieldName = Xrm.Page.getAttribute("op_field_name").getValue();

            if (entityName != null && fieldName != null) {

                var uniqueName = entityName + " - " + fieldName;

                //Unique Name Duplicate Detection
                _uniqueNameDuplicateDetection(executionObj, uniqueName);

                //set Unique Name
                Xrm.Page.getAttribute("op_unique_name").setValue(uniqueName);
            }
            else
            {
                //clear Unique Name
                Xrm.Page.getAttribute("op_unique_name").setValue(null);
            }
        }
    }
}

function _validateFormat()
    ///<summary>Validate Auto Number Format. Correct formats examples: {0}, A-{0}, A-{0}-B</summary>
    ///<return>Error message</return>
{

    //get Format
    var autoNumberFormat = Xrm.Page.getAttribute("op_format").getValue();
    if (autoNumberFormat != null)
    {
        var format = autoNumberFormat.indexOf('{0}');

        //Format is incorrect
        if (format == -1)
        {
            //Display error message
            var errorMsg = "Presented Format '" + autoNumberFormat + "' is incorrect. Please insert correct Format like {0} or prefix{0} or prefix{0}suffix.";
            _showError(errorMsg, "op_format");
        }
    }
}