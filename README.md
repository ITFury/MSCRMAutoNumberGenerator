## MS CRM Auto Number Generator


**MS CRM Auto Number Generator** add-on for Microsoft Dynamics CRM 2016 OnPremise allows automatic generate number for entities across the CRM system. 
The format and length of number can be customized. 

You can generate:

- Whole number (Sample: **1, 01, 00001**);
- Number with prefix (Sample: **Auto-1, Auto-01, Auto-00001**);
- Number with suffix (Sample: **1-Number, 01-Number, 00001-Number**);
- Number with prefix and suffix (Sample: **Auto-1-Number, Auto-01-Number, Auto-00001-Number**).

Add-on have possibility to override auto number on Delete event. It don't allow to update auto number manually on Update event.


## Getting Started

### Prerequisites

- Install Microsoft Dynamics CRM 2016 OnPremise;
- Install Microsoft Dynamics CRM SDK to use *Plugin Registration Tool*;
- You must have  *System Customizer* Security Role in Microsoft Dynamics CRM.

### Installing

To install **MS CRM Auto Number Generator** add-on, import `AutoNumberGenerator_1_0_0_0.zip` solution to Microsoft Dynamics CRM 2016:

1. Navigate to **Settings -> Solutions**.
2. Click **Import** button on action bar.
3. In the **Select Solution Package** screen click **Choose File** button and select `AutoNumberGenerator_1_0_0_0.zip`. Click **Next**.

![ImportSolution_1](https://user-images.githubusercontent.com/31651093/30382323-7b1f1b8a-98a7-11e7-9c3f-ac567fe14663.png)

4. In the **Solution Information** screen click **Next**.
5. In the **Import Options** screen click **Import**.
6. In the **Importing Solution** screen click **Publish All Customizations**.

![ImportSolution_2](https://user-images.githubusercontent.com/31651093/30382335-8147f77a-98a7-11e7-8065-be2121e4dc0b.png)

7. Click **Close**.

Imported Solution include:

- **Auto Number Config** (`op_auto_number_config`) entity to configure auto number generation;
- **Auto Number (test)** (`op_auto_number_test`) entity to test auto number generation;
- Registered `OP.MSCRM.AutoNumberGenerator.Plugins` assembly with **Create, Update, Delete** steps on `op_auto_number_test` entity.

**Create** - create Auto Number in sequence order.

**Update** - don't allow to update Auto Number manually.

**Delete** - override Auto Number if **Auto Number Config** entity field **Override Sequence** is set to **Yes**. 

*Delete event sample:*

| Generated number | Deleted number | Number after delete |
| - | - | - |
| 01 |  | 01 |
| 02 |  | 02 |
| 03 | 03 |    |
| 04 |  | 03 |
| 05 |  | 04 |


**Configure Your Own Auto Number**

1. In Microsoft Dynamics CRM navigate to **Settings -> Auto Number Configs**.
2. In navigation bar click **NEW**.
3. Input Your data.

![AutoNumberConfig](https://user-images.githubusercontent.com/31651093/30382369-9cc7dcc2-98a7-11e7-8cee-69dfc838f27a.png)

4. Click **SAVE**.

**Register Plugin on own Entity**

To register `OP.MSCRM.AutoNumberGenerator.Plugins` assembly on Your entity, open **Plugin Registration Tool** on Your organization.
1. In organization tab on action bar choose **Register -> Register New Assembly**.
2. In the **Register New Assembly** screen click **...** button and select `OP.MSCRM.AutoNumberGenerator.Plugins.dll`. Check **Select All / Deselect All** checkbox. Check islotaion mode **None** and assembly location **Database**. Click **Register Selected Plugins**.

![PluginRegistrationTool_Assembly](https://user-images.githubusercontent.com/31651093/30382395-a72da732-98a7-11e7-8e31-af8a39bed15e.png)

3. In organization tab right click on **(Plugin)OP.MSCRM.AutoNumbergenerator.Plugins.Plugins.GenerateAutoNumberPlugin -> Register New Step.**

![PluginRegistrationTool_Step](https://user-images.githubusercontent.com/31651093/30382406-ae17ecce-98a7-11e7-95d8-166a75c62cb8.png)

4. Register **Create** step on Your **Primary Entity**.

![PluginRegistrationTool_Create](https://user-images.githubusercontent.com/31651093/30382407-aee370ba-98a7-11e7-858a-b6c153d41c17.png)

5. Register **Update** step on Your **Primary Entity**.

![PluginRegistrationTool_Update](https://user-images.githubusercontent.com/31651093/30382412-b1566c58-98a7-11e7-935b-7fcc0f9c1213.png)

To improve step performance, in **Filtering Attributes** field input field name where display generated Auto Number.

6. Register **Delete** step on Your **Primary Entity**.

![PluginRegistrationTool_Delete](https://user-images.githubusercontent.com/31651093/30382414-b33ff5e8-98a7-11e7-9d1d-ef0d002d6836.png)


**Auto Number generation DEMO**

- Generated Auto Numbers on Create event.

![AutoNumberTest_Create](https://user-images.githubusercontent.com/31651093/30382377-9f42d88a-98a7-11e7-956e-dc8cfc32540d.png)


- Auto Number override sample on Delete event.

![AutoNumberTest_Delete1](https://user-images.githubusercontent.com/31651093/30382381-a0c6fe66-98a7-11e7-873f-80775121b27d.png)

![AutoNumberTest_Delete2](https://user-images.githubusercontent.com/31651093/30382384-a2c1219c-98a7-11e7-8622-3b68cfbff337.png)

## Running the tests

To run **Unit Tests**, open `OP.MSCRM.AutoNumberGenerator.sln` in Visual Studio 2015.
1. To connect to Your MS CRM, in `MSCRMHelper.cs` file change `OrgService` property credentials and organization URI to personal data.

![picture](https://user-images.githubusercontent.com/31651093/30382417-b5c2b38c-98a7-11e7-9d56-688d5891e4d3.png)

2. If You want to test own entity, then open `GenerateAutoNumberManagerCRMServicesTest.cs` file and change `entityLogicalName` value to Your entity logical name and `entityAttributeName` to Your entity attribute name.

![picture](https://user-images.githubusercontent.com/31651093/30382422-b72b2d1c-98a7-11e7-853e-0204fd3b0b60.png)

3. In Visual Studio on navigation bar click **Test -> Run -> All Tests**.

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/ITFury/MSCRMAutoNumberGenerator/blob/master/LICENSE) file for details.