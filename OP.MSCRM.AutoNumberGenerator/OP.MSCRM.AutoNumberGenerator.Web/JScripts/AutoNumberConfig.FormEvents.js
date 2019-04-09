//op_autonumberconfig entity Form events
//Related AutoNumberGenerator.FormsManager.js


/*---EVENT HANDLERS---*/

function op_autonumberconfigOnLoad()
{
    formInit();
    _setDefaultValues();
    
    _setDisabled();
}


function op_autonumberconfigOnSave(executionObj) {
    _setUniqueName(executionObj);
}


function op_entityschemanameOnChange()
{
    _validatePresentedEntity();

    var executionObj = null;
    _setUniqueName(executionObj);
}


function op_fieldschemanameOnChange()
{
    _validatePresentedEntity();

    var executionObj = null;
    _setUniqueName(executionObj);
}


function op_startOnChange() {
    _setSequence();
    _setNumberPreview();
}


function op_lengthOnChange() {
    _setNumberPreview();
}


function op_prefixOnChange() {
    _setNumberPreview();
}


function op_suffixOnChange() {
    _setNumberPreview();
}


/*---CONSTANTS---*/

const defaultValue = {
    start: "1",
    increment: "1",
    length: "1"
};

const fieldSchemaName = {
    name: "op_name",
    entitySchemaName: "op_entityschemaname",
    fieldSchemaName: "op_fieldschemaname",
    start: "op_start",
    increment: "op_increment",
    length: "op_length",
    sequence: "op_sequence",
    prefix: "op_prefix",
    suffix: "op_suffix",
    preview: "op_preview"
};

const entitySchemaName = {
    systemForm: "SystemForm",
    autoNumberConfig: "op_autonumberconfig"
};

const message = {
    fieldNotExist: "Field '{0}' of Entity '{1}' doesn't exist.",
    entityNotExist: "Entity '{0}' doesn't exist.",
    uniqueNameExist: "Entity Unique Name '{0}' already exist. Please change Entity Schema Name or Field Schema Name value."
};


/*---FUNCTIONS---*/

function _setNumberPreview() {

    if (isCreateForm)
    {
        var startNumber = Xrm.Page.getAttribute(fieldSchemaName.start).getValue() == null
            ? defaultValue.start
            : Xrm.Page.getAttribute(fieldSchemaName.start).getValue();

        var numberLength = Xrm.Page.getAttribute(fieldSchemaName.length).getValue() == null
            ? defaultValue.length
            : Xrm.Page.getAttribute(fieldSchemaName.length).getValue();

        var numberPrefix = Xrm.Page.getAttribute(fieldSchemaName.prefix).getValue() == null
            ? ""
            : Xrm.Page.getAttribute(fieldSchemaName.prefix).getValue();

        var numberSuffix = Xrm.Page.getAttribute(fieldSchemaName.suffix).getValue() == null
            ? ""
            : Xrm.Page.getAttribute(fieldSchemaName.suffix).getValue();

        if (startNumber && numberLength)
        {
            //Format number by length. If number is 2 and length is 4, then formatted number is 0002.
            number = Array(numberLength - String(startNumber).length + 1).join('0') + startNumber;

            var numberPreview = String.format("{0}{1}{2}", numberPrefix, number, numberSuffix);

            Xrm.Page.getAttribute(fieldSchemaName.preview).setValue(numberPreview);
        }
    }
}


function _setSequence() {
///<summary>Set current sequence</summary>
    if (isCreateForm) {
        var startNumber = Xrm.Page.getAttribute(fieldSchemaName.start).getValue();
        if (startNumber != null)
        {
            Xrm.Page.getAttribute(fieldSchemaName.sequence).setValue(startNumber);
        }
    }
}

function _setDefaultValues()
///<summary>Set fields default values</summary>
{
    //If the form is in Create state set fields default values
    if (isCreateForm) {
        var startNumber = Xrm.Page.getAttribute(fieldSchemaName.start).getValue();
        if (startNumber == null) {
            Xrm.Page.getAttribute(fieldSchemaName.start).setValue(defaultValue.start);
        }

        var numberIncrement = Xrm.Page.getAttribute(fieldSchemaName.increment).getValue();
        if (numberIncrement == null) {
            Xrm.Page.getAttribute(fieldSchemaName.increment).setValue(defaultValue.increment);
        }

        var numberLength = Xrm.Page.getAttribute(fieldSchemaName.length).getValue();
        if (numberLength == null) {
            Xrm.Page.getAttribute(fieldSchemaName.length).setValue(defaultValue.length);
        }

        var sequence = Xrm.Page.getAttribute(fieldSchemaName.sequence).getValue();
        if ((startNumber != null && sequence == null) || sequence < startNumber) {
            Xrm.Page.getAttribute(fieldSchemaName.sequence).setValue(startNumber);
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
        Xrm.Page.getControl(fieldSchemaName.entitySchemaName).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.fieldSchemaName).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.start).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.increment).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.length).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.prefix).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.suffix).setDisabled(isUpdateForm);
        Xrm.Page.getControl(fieldSchemaName.preview).setDisabled(isUpdateForm);
    }
}


function _showError(errorMsg, fieldName)
    ///<summary>Display error message</summary>
    ///<param name="errorMsg" type="String">Error message</param>
    ///<param name="fieldName" type="String">Entity field name that need to clear</param>
{

    //Display error message
    showModalDialog(errorMsg);

    //Clear field value
    Xrm.Page.getAttribute(fieldName).setValue(null);
}


function _validatePresentedEntity()
    ///<summary>Validate presented Entity Name and Entity Field existence. If doesn't exist then display error message.</summary>
    ///<return>Error message</return>
{

    //get op_entityschemaname field value
    var searchEntityName = Xrm.Page.getAttribute(fieldSchemaName.entitySchemaName).getValue();
   
    if (searchEntityName != null)
    {
        var entityName = entitySchemaName.systemForm;
        var searchFieldNames = ["ObjectTypeCode"];
        var searchFieldValues = [searchEntityName.toLowerCase()];
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
                    //get op_fieldschemaname field value
                    var fieldName = Xrm.Page.getAttribute(fieldSchemaName.fieldSchemaName).getValue();
                    if (fieldName != null) {
                        var fieldExist = formXml.indexOf('"' + fieldName.toLowerCase() + '"');

                        //Field exist. Don't continue.
                        if (fieldExist != -1) {
                            return;
                        }
                        //Field of presented Entity Form doesn't exist
                        else if (i == systemForm.length - 1 && fieldExist == -1)
                        {
                            //Display error message
                            errorMsg = String.format(message.fieldNotExist, fieldName, searchEntityName);
                            _showError(errorMsg, fieldSchemaName.fieldSchemaName);
                        }
                    }
                }
            }
        }
        //Presented Entity Name doesn't exist
        else
        {
            //Display error message
            errorMsg = String.format(message.entityNotExist, searchEntityName);
            _showError(errorMsg, fieldSchemaName.entitySchemaName);
        }
    }
}


function _uniqueNameDuplicateDetection(executionObj, uniqueName)
    ///<summary>Detect Unique Name duplicate</summary>
    ///<param name="executionObj" type="object">Execution object</param>
    ///<param name="uniqueName" type="String">Unique entity name. Example: account-accountnumber</param>
    ///<return>Error message</return>
{

    var entityName = entitySchemaName.autoNumberConfig;
    var searchFieldNames = [fieldSchemaName.name];
    var searchFieldValues = [uniqueName];
    var searchOperator = ["eq"];
    var fieldsToReturn = [fieldSchemaName.name];
    var recordsToReturn = "";

    var autoNumberConfig = retrieveEntityByQuery(entityName, searchFieldNames, searchFieldValues, searchOperator, fieldsToReturn, recordsToReturn);

    if (autoNumberConfig != null && autoNumberConfig.length > 0)
    {
        //Display error message
        errorMsg = String.format(message.uniqueNameExist, uniqueName);
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

        if (Xrm.Page.getAttribute(fieldSchemaName.entitySchemaName) != null
            && Xrm.Page.getAttribute(fieldSchemaName.fieldSchemaName) != null)
        {
            //get values
            var entityName = Xrm.Page.getAttribute(fieldSchemaName.entitySchemaName).getValue();
            var fieldName = Xrm.Page.getAttribute(fieldSchemaName.fieldSchemaName).getValue();

            if (entityName != null && fieldName != null) {

                var uniqueName = String.format("{0} - {1}", entityName, fieldName);

                //Unique Name Duplicate Detection
                _uniqueNameDuplicateDetection(executionObj, uniqueName);

                //set Unique Name
                Xrm.Page.getAttribute(fieldSchemaName.name).setValue(uniqueName);
            }
            else
            {
                //clear Unique Name
                Xrm.Page.getAttribute(fieldSchemaName.name).setValue(null);
            }
        }
    }
}