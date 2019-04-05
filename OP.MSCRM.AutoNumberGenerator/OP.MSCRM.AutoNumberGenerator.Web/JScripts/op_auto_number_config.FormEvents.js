//op_auto_number_config entity Form events
//Related AutoNumberGenerator.FormsManager.js


/*---EVENT HANDLERS---*/

function op_auto_number_configOnLoad()
{
    formInit();
    _setDefaultValues();
    
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


function op_start_numberOnChange() {

    _setSequence();
    _setNumberPreview();
}


function op_number_lengthOnChange() {
    _setNumberPreview();
}


function op_number_prefixOnChange() {
    _setNumberPreview();
}


function op_number_suffixOnChange() {
    _setNumberPreview();
}


/*---CONSTANTS---*/
const startNumberDefault = "1";
const numberStepDefault = "1";
const numberLengthDefault = "1";



/*---FUNCTIONS---*/

function _setNumberPreview() {

    if (isCreateForm)
    {

        var startNumber = Xrm.Page.getAttribute("op_start_number").getValue() == null ? startNumberDefault : Xrm.Page.getAttribute("op_start_number").getValue();
        var numberLength = Xrm.Page.getAttribute("op_number_length").getValue() == null ? numberLengthDefault : Xrm.Page.getAttribute("op_number_length").getValue();
        var numberPrefix = Xrm.Page.getAttribute("op_number_prefix").getValue() == null ? "" : Xrm.Page.getAttribute("op_number_prefix").getValue();
        var numberSuffix = Xrm.Page.getAttribute("op_number_suffix").getValue() == null ? "" : Xrm.Page.getAttribute("op_number_suffix").getValue();

        if (startNumber && numberLength)
        {
            //Format number by length. If number is 2 and length is 4, then formatted number is 0002.
            number = Array(numberLength - String(startNumber).length + 1).join('0') + startNumber;

            var numberPreview = numberPrefix + number + numberSuffix;
            Xrm.Page.getAttribute("op_number_preview").setValue(numberPreview);
        }
    }
}


function _setSequence() {

    if (isCreateForm) {
        var startNumber = Xrm.Page.getAttribute("op_start_number").getValue();
        if (startNumber != null)
        {
            Xrm.Page.getAttribute("op_sequence").setValue(startNumber);
        }
    }
}

function _setDefaultValues()
///<summary>Set field default values</summary>
{
    //If the form is in Create state set fields default values
    if (isCreateForm) {
        var startNumber = Xrm.Page.getAttribute("op_start_number").getValue();
        if (startNumber == null) {
            Xrm.Page.getAttribute("op_start_number").setValue(startNumberDefault);
        }

        var numberStep = Xrm.Page.getAttribute("op_number_step").getValue();
        if (numberStep == null) {
            Xrm.Page.getAttribute("op_number_step").setValue(numberStepDefault);
        }

        var numberLength = Xrm.Page.getAttribute("op_number_length").getValue();
        if (numberLength == null) {
            Xrm.Page.getAttribute("op_number_length").setValue(numberLengthDefault);
        }

        var sequence = Xrm.Page.getAttribute("op_sequence").getValue();
        if ((startNumber != null && sequence == null) || sequence < startNumber) {
            Xrm.Page.getAttribute("op_sequence").setValue(startNumber);
        }

    }

    _setSequence();
    _setNumberPreview();
}


function _setDisabled()
///<summary>Set field behavior to read-only mode</summary>
{   
    //If the form is in Update state set fields to read-only
    if (isUpdateForm)
    {
        Xrm.Page.getControl("op_entity_name").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_field_name").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_start_number").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_number_step").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_number_length").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_number_prefix").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_number_suffix").setDisabled(isUpdateForm);
        Xrm.Page.getControl("op_number_preview").setDisabled(isUpdateForm);
    }
}


function _showError(errorMsg, fieldName)
    ///<summary>Display error message</summary>
    ///<param name="errorMsg" type="String">Error message</param>
    ///<param name="fieldName" type="String">Entity field name that need to clear</param>
{

    //Display error message
    showModalDialog(errorMsg);

    //clear field value
    Xrm.Page.getAttribute(fieldName).setValue(null);
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
                            errorMsg = "Field '" + fieldName + "' of Entity '" + searchEntityName + "' doesn't exist.";
                            _showError(errorMsg, "op_field_name");
                        }
                    }
                }
            }
        }
        //Presented Entity Name doesn't exist
        else
        {
            //Display error message
            errorMsg = "Entity '" + searchEntityName + "' doesn't exist.";
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
        errorMsg = "Entity Unique Name '" + uniqueName + "' already exist. Please change Entity Name or Entity Field value.";
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