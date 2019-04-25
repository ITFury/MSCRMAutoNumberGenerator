## Microsoft Dymanics 365 CRM Auto-Number Generator


**Microsoft Dymanics 365 CRM Auto-Number Generator** add-on for Online allows automatic generate number for entities and fields across the CRM system. 
It allows to specify format of Auto-Number: set the start number, increment, prefix in any symbols, number length, suffix in any symbols. 

You can generate:

- Whole number (Sample: **1, 00001, 0000030**);
- Number with prefix (Sample: **A4q?&-1, A4q?&01, A4q?&-0000030**);
- Number with suffix (Sample: **1-Bio!, 01Bio!, 00001-Bio!**);
- Number with prefix and suffix (Sample: **A4q?&-1-Bio!, A4q?&01Bio!, A4q?&-00001-Bio!**).

Auto-Number Generator have possibility to rearrange Auto-Number in display entity under the following conditions:
- triggered  **Delete** event;
- **Rearrange Sequence After Delete** field value on **Auto-Number Configuration** form is set to **Yes**. 

It don't allows to update Auto-Number manually on the form on **Update** event. In result an appropriate error message will appear.


## Getting Started

### Prerequisites

- Install Microsoft Dynamics 365 CRM Online;
- Download Dynamics 365 for Customer Engagement apps Software Development Kit (SDK) to use *Plugin Registration Tool*;
- You should have *System Customizer* Security Role in Microsoft Dynamics 365.


### Installing

To install **MS CRM Auto Number Generator**, import `AutoNumberGenerator_1_0_0_0.zip` solution to Microsoft Dynamics 365 CRM Online:

1. Navigate to **Settings -> Solutions**.
2. Click **Import** button on action bar.
3. In the **Select Solution Package** screen click **Choose File** button and select `AutoNumberGenerator_1_0_0_0.zip`. Click **Next**.
4. In the **Solution Information** screen click **Next**.
5. In the **Import Options** screen click **Import**.
6. In the **Importing Solution** screen click **Publish All Customizations**.
7. Click **Close**.

Imported Solution include:

- **Auto-Number Configuration** (`op_autonumberconfig`) entity to configure auto number generation;
- **Auto-Number Test** (`op_autonumbertest`) entity to test auto number generation;
- Registered `OP.MSCRM.AutoNumberGenerator.Plugins` assembly with **Create, Update, Delete** steps on `op_autonumbertest` entity.

**Create** - create Auto-Number in provided format.

**Update** - don't allows to update Auto-Number manually.

**Delete** - delete or delete and rearrange sequence of existing Auto-Numbers. 

*Sample of Delete event with rearrange:*

| Generated Auto-Number | Deleted Auto-Number | Rearranged Auto-Number |
| - | - | - |
| A-001 |  | A-001 |
| A-002 |  | A-002 |
| A-003 | A-003 |    |
| A-004 |  | A-003 |
| A-005 |  | A-004 |


**Configure Your Own Auto-Number**

1. In Microsoft Dynamics CRM navigate to **Settings -> Auto-Number Configurations**.
2. In navigation bar click **NEW**.
3. Input Your data.

![AutoNumberConfiguration](https://user-images.githubusercontent.com/31651093/55780182-0d931e00-5ab0-11e9-994f-9614e5fe596c.png)

4. Click **SAVE**.


**Register Plugin on own Entity**

To register `OP.MSCRM.AutoNumberGenerator.Plugins` assembly on Your entity, open **Plugin Registration Tool** on Your organization.
1. In organization tab on action bar choose **Register -> Register New Assembly**.
2. In the **Register New Assembly** screen click **...** button and select `OP.MSCRM.AutoNumberGenerator.Plugins.dll`. Check **Select All / Deselect All** checkbox. Check islotaion mode **None** and assembly location **Database**. Click **Register Selected Plugins**.

![RegisterAssembly](https://user-images.githubusercontent.com/31651093/55780196-0ec44b00-5ab0-11e9-854b-6ca85858e9f5.png)

3. In organization tab right click on **(Plugin)OP.MSCRM.AutoNumbergenerator.Plugins.Plugins.GenerateAutoNumberPlugin -> Register New Step.**

![RegisterNewStep](https://user-images.githubusercontent.com/31651093/55780197-0ec44b00-5ab0-11e9-970a-456d4b6ada15.png)

4. Register **Create** step on Your **Primary Entity**.

![CreateStep](https://user-images.githubusercontent.com/31651093/55780184-0e2bb480-5ab0-11e9-82df-527efbff899d.png)

5. Register **Update** step on Your **Primary Entity**.

![UpdateStep](https://user-images.githubusercontent.com/31651093/55780198-0f5ce180-5ab0-11e9-80fc-9ad7464dcb4c.png)

To improve step performance, in **Filtering Attributes** field input field name where display generated Auto-Number.

6. Register **Delete** step on Your **Primary Entity**.

![DeleteStep](https://user-images.githubusercontent.com/31651093/55780186-0e2bb480-5ab0-11e9-804f-016d9208dc2a.png)


**Auto-Number generation DEMO**

- Generated Auto-Numbers on Create event.

![DemoCreate](https://user-images.githubusercontent.com/31651093/56734663-20724780-676c-11e9-8c23-bec0237a57fa.png)


- Auto-Number behavior on Update event.

![DemoUpdate](https://user-images.githubusercontent.com/31651093/56734674-2cf6a000-676c-11e9-8409-f10098d7685e.png)


- Auto-Number behavior on Delete event with rearrange.

Before delete:

![DemoDelete](https://user-images.githubusercontent.com/31651093/56734680-31bb5400-676c-11e9-807d-064c5d2c525d.png)

After delete:

![DemoDeleteRearrange](https://user-images.githubusercontent.com/31651093/56734685-36800800-676c-11e9-8f5a-4aef5e17247c.png)


## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/ITFury/MSCRMAutoNumberGenerator/blob/master/LICENSE) file for details.
