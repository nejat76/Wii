#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace WBFSSync;
using namespace System::IO;


namespace FE100 {

	/// <summary>
	/// Summary for WBFSDrive
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class WBFSDrive : public System::Windows::Forms::Form
	{
	public:
		WBFSDrive(void)
		{
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~WBFSDrive()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::ComboBox^  cmbDriveList;

	protected: 
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::GroupBox^  groupBox1;
	private: System::Windows::Forms::Button^  button1;
	private: System::Windows::Forms::Button^  btnRefresh;
	private: System::Windows::Forms::ListView^  listGames;
	private: System::Windows::Forms::ColumnHeader^  name;
	private: System::Windows::Forms::ColumnHeader^  id;
	private: System::Windows::Forms::ColumnHeader^  code;
	private: System::Windows::Forms::ColumnHeader^  size;
	private: System::Windows::Forms::Button^  btnCreateSelected;
	private: System::Windows::Forms::Panel^  panel1;



	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			this->cmbDriveList = (gcnew System::Windows::Forms::ComboBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->groupBox1 = (gcnew System::Windows::Forms::GroupBox());
			this->listGames = (gcnew System::Windows::Forms::ListView());
			this->id = (gcnew System::Windows::Forms::ColumnHeader());
			this->code = (gcnew System::Windows::Forms::ColumnHeader());
			this->name = (gcnew System::Windows::Forms::ColumnHeader());
			this->size = (gcnew System::Windows::Forms::ColumnHeader());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->btnRefresh = (gcnew System::Windows::Forms::Button());
			this->btnCreateSelected = (gcnew System::Windows::Forms::Button());
			this->panel1 = (gcnew System::Windows::Forms::Panel());
			this->groupBox1->SuspendLayout();
			this->panel1->SuspendLayout();
			this->SuspendLayout();
			// 
			// cmbDriveList
			// 
			this->cmbDriveList->FormattingEnabled = true;
			this->cmbDriveList->Location = System::Drawing::Point(107, 3);
			this->cmbDriveList->Name = L"cmbDriveList";
			this->cmbDriveList->Size = System::Drawing::Size(115, 21);
			this->cmbDriveList->TabIndex = 0;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(12, 6);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(89, 13);
			this->label1->TabIndex = 1;
			this->label1->Text = L"Select drive letter";
			// 
			// groupBox1
			// 
			this->groupBox1->Controls->Add(this->listGames);
			this->groupBox1->Location = System::Drawing::Point(15, 30);
			this->groupBox1->Name = L"groupBox1";
			this->groupBox1->Size = System::Drawing::Size(360, 254);
			this->groupBox1->TabIndex = 2;
			this->groupBox1->TabStop = false;
			this->groupBox1->Text = L"Game List";
			// 
			// listGames
			// 
			this->listGames->Columns->AddRange(gcnew cli::array< System::Windows::Forms::ColumnHeader^  >(4) {this->id, this->code, this->name, 
				this->size});
			this->listGames->FullRowSelect = true;
			this->listGames->Location = System::Drawing::Point(7, 19);
			this->listGames->MultiSelect = false;
			this->listGames->Name = L"listGames";
			this->listGames->Size = System::Drawing::Size(344, 215);
			this->listGames->TabIndex = 1;
			this->listGames->UseCompatibleStateImageBehavior = false;
			this->listGames->View = System::Windows::Forms::View::Details;
			// 
			// id
			// 
			this->id->Tag = L"id";
			this->id->Text = L"#";
			this->id->Width = 31;
			// 
			// code
			// 
			this->code->Tag = L"code";
			this->code->Text = L"Disc Id";
			// 
			// name
			// 
			this->name->Tag = L"name";
			this->name->Text = L"Name";
			this->name->Width = 142;
			// 
			// size
			// 
			this->size->Tag = L"size";
			this->size->Text = L"Size";
			this->size->Width = 102;
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(228, 1);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(75, 23);
			this->button1->TabIndex = 3;
			this->button1->Text = L"Get List";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &WBFSDrive::button1_Click);
			// 
			// btnRefresh
			// 
			this->btnRefresh->Location = System::Drawing::Point(309, 1);
			this->btnRefresh->Name = L"btnRefresh";
			this->btnRefresh->Size = System::Drawing::Size(95, 23);
			this->btnRefresh->TabIndex = 4;
			this->btnRefresh->Text = L"Refresh drive list";
			this->btnRefresh->UseVisualStyleBackColor = true;
			this->btnRefresh->Click += gcnew System::EventHandler(this, &WBFSDrive::btnRefresh_Click);
			// 
			// btnCreateSelected
			// 
			this->btnCreateSelected->Location = System::Drawing::Point(88, 300);
			this->btnCreateSelected->Name = L"btnCreateSelected";
			this->btnCreateSelected->Size = System::Drawing::Size(172, 23);
			this->btnCreateSelected->TabIndex = 5;
			this->btnCreateSelected->Text = L"Create Channel";
			this->btnCreateSelected->UseVisualStyleBackColor = true;
			this->btnCreateSelected->Click += gcnew System::EventHandler(this, &WBFSDrive::btnCreateSelected_Click);
			// 
			// panel1
			// 
			this->panel1->Controls->Add(this->cmbDriveList);
			this->panel1->Controls->Add(this->btnCreateSelected);
			this->panel1->Controls->Add(this->label1);
			this->panel1->Controls->Add(this->btnRefresh);
			this->panel1->Controls->Add(this->groupBox1);
			this->panel1->Controls->Add(this->button1);
			this->panel1->Location = System::Drawing::Point(427, 15);
			this->panel1->Name = L"panel1";
			this->panel1->Size = System::Drawing::Size(409, 367);
			this->panel1->TabIndex = 6;
			// 
			// WBFSDrive
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(911, 578);
			this->Controls->Add(this->panel1);
			this->Name = L"WBFSDrive";
			this->Text = L"WBFSDrive";
			this->Load += gcnew System::EventHandler(this, &WBFSDrive::WBFSDrive_Load);
			this->groupBox1->ResumeLayout(false);
			this->panel1->ResumeLayout(false);
			this->panel1->PerformLayout();
			this->ResumeLayout(false);

		}
#pragma endregion
	private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) {
				 if (cmbDriveList->SelectedItem!=nullptr) 
				 {
					 String^ selectedDriveText = (String^) cmbDriveList->SelectedItem;
					 String^ selectedDrive = selectedDriveText->Substring(0,1);
					 WBFSDevice^ device = gcnew WBFSDevice();
					 int returnCode = device->IsWBFSDrive(selectedDrive, false);
					 
					 if (returnCode == (int)WBFSRet::RET_WBFS_OK)
					 {
						 device->Open(selectedDrive, false);
						 int count = device->DiscCount;
						 for (int i=0;i<count;i++) 
						 {
							 IDisc^ disc = device->GetDiscByIndex(i);
							 ListViewItem^ item = gcnew ListViewItem(i.ToString());
							 listGames->Items->Add(item);
							 listGames->Items[i]->SubItems->Add(disc->Code);
							 listGames->Items[i]->SubItems->Add(disc->Name);
							 listGames->Items[i]->SubItems->Add(sizeToGB(disc->WBFSSize));
							 //Button^ button = gcnew Button();
							 //button->Text = "Use";
							 //button->Click+= gcnew System::EventHandler(this, &WBFSDrive::ButtonHandler);
							 //listGames->Items[i]->SubItems->Add(new ListViewItem::ListViewSubItem(
						 }
					 }

					 device->Close();
				 } else 
				 {
					 MessageBox::Show("Oops, select a drive first", "Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
				 }

	}

	private: System::Void ButtonHandler(System::Object^  sender, System::EventArgs^  e) 
			 {
				 MessageBox::Show(sender->ToString() + " clicked me", "Heya", MessageBoxButtons::OK, MessageBoxIcon::Information);
			 }

	private: System::Void WBFSDrive_Load(System::Object^  sender, System::EventArgs^  e) {
				 refreshDrives();
		 }

   private: System::Void refreshDrives() 
			{
				array<System::IO::DriveInfo^>^ drives = System::IO::DriveInfo::GetDrives();
				cmbDriveList->Items->Clear();
				for (int i=0;i<drives->Length;i++) 
				{
					cmbDriveList->Items->Add(drives[i]->Name);
					//cmbDriveList->Items->Add(drives[i]->Name + " - " + drives[i]->VolumeLabel);
				}
			}
private: System::Void btnRefresh_Click(System::Object^  sender, System::EventArgs^  e) {
			 refreshDrives();
		 }

private: System::String^ sizeToGB(double size) {
			 double x = size/(1024*1024*1024);
			 return x.ToString("0.000") + " GB";

		 }
private: System::Void btnCreateSelected_Click(System::Object^  sender, System::EventArgs^  e) 
		 {
			 if (listGames->SelectedItems->Count == 0) 
			 {
				MessageBox::Show("Please select a game from the list!", "Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
				return;
			 }
			 String^ selectedGame = listGames->SelectedItems[0]->SubItems[2]->Text;
			 MessageBox::Show(String::Format("Now returning to the main window for your selected game {0}", selectedGame), "Heya", MessageBoxButtons::OK, MessageBoxIcon::Information);			 
		 }
};
}
