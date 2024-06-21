#pragma once

#include "savetools.h"
#include "InfoForm.h"

namespace FE100 {

	using namespace System;
	using namespace System::ComponentModel;
	using namespace System::Collections;
	using namespace System::Windows::Forms;
	using namespace System::Data;
	using namespace System::Drawing;
	using namespace System::Runtime::InteropServices;

	/// <summary>
	/// Summary for Form1
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class Form1 : public System::Windows::Forms::Form
	{
	public:
		Form1(System::AppDomain^ appDomain)
		{
			this->appDomain = appDomain;
			InitializeComponent();
			//
			//TODO: Add the constructor code here
			//
		}

		//Form1(void)
		//{
		//	InitializeComponent();
		//	//
		//	//TODO: Add the constructor code here
		//	//
		//}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~Form1()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  txtDataFile;
	protected: 

	private: System::Windows::Forms::Label^  lblFileName;
	private: System::Windows::Forms::Button^  btnUnpack;

	private: System::Windows::Forms::Button^  button2;
	private: System::Windows::Forms::Button^  btnBrowse;
	private: FE100::InfoForm^ frm;
	private: System::AppDomain^ appDomain;
	private: System::Windows::Forms::FolderBrowserDialog^  openSaveFileDlg;
	private: String^ openFileName;
	private: System::Windows::Forms::OpenFileDialog^  openSaveFileDialog;
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::TextBox^  txtSaveDirectory;
	private: System::Windows::Forms::Button^  btnOpenSaveFolder;
	private: System::Windows::Forms::Button^  button1;
	private: System::Windows::Forms::CheckBox^  chkUseBannerBin;


	protected: 

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
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(Form1::typeid));
			this->txtDataFile = (gcnew System::Windows::Forms::TextBox());
			this->lblFileName = (gcnew System::Windows::Forms::Label());
			this->btnUnpack = (gcnew System::Windows::Forms::Button());
			this->button2 = (gcnew System::Windows::Forms::Button());
			this->btnBrowse = (gcnew System::Windows::Forms::Button());
			this->openSaveFileDlg = (gcnew System::Windows::Forms::FolderBrowserDialog());
			this->openSaveFileDialog = (gcnew System::Windows::Forms::OpenFileDialog());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->txtSaveDirectory = (gcnew System::Windows::Forms::TextBox());
			this->btnOpenSaveFolder = (gcnew System::Windows::Forms::Button());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->chkUseBannerBin = (gcnew System::Windows::Forms::CheckBox());
			this->SuspendLayout();
			// 
			// txtDataFile
			// 
			this->txtDataFile->Location = System::Drawing::Point(156, 37);
			this->txtDataFile->Name = L"txtDataFile";
			this->txtDataFile->Size = System::Drawing::Size(301, 20);
			this->txtDataFile->TabIndex = 0;
			// 
			// lblFileName
			// 
			this->lblFileName->AutoSize = true;
			this->lblFileName->Location = System::Drawing::Point(2, 41);
			this->lblFileName->Name = L"lblFileName";
			this->lblFileName->Size = System::Drawing::Size(152, 13);
			this->lblFileName->TabIndex = 1;
			this->lblFileName->Text = L"Savefile to unpack (data.bin) : ";
			// 
			// btnUnpack
			// 
			this->btnUnpack->Location = System::Drawing::Point(508, 35);
			this->btnUnpack->Name = L"btnUnpack";
			this->btnUnpack->Size = System::Drawing::Size(97, 25);
			this->btnUnpack->TabIndex = 2;
			this->btnUnpack->Text = L"Unpack";
			this->btnUnpack->UseVisualStyleBackColor = true;
			this->btnUnpack->Click += gcnew System::EventHandler(this, &Form1::button1_Click);
			// 
			// button2
			// 
			this->button2->Location = System::Drawing::Point(587, 120);
			this->button2->Name = L"button2";
			this->button2->Size = System::Drawing::Size(36, 29);
			this->button2->TabIndex = 3;
			this->button2->Text = L"info";
			this->button2->UseVisualStyleBackColor = true;
			this->button2->Click += gcnew System::EventHandler(this, &Form1::button2_Click);
			// 
			// btnBrowse
			// 
			this->btnBrowse->Location = System::Drawing::Point(462, 35);
			this->btnBrowse->Name = L"btnBrowse";
			this->btnBrowse->Size = System::Drawing::Size(33, 25);
			this->btnBrowse->TabIndex = 4;
			this->btnBrowse->Text = L"...";
			this->btnBrowse->UseVisualStyleBackColor = true;
			this->btnBrowse->Click += gcnew System::EventHandler(this, &Form1::btnBrowse_Click);
			// 
			// openSaveFileDialog
			// 
			this->openSaveFileDialog->DefaultExt = L"bin";
			this->openSaveFileDialog->FileName = L"data.bin";
			this->openSaveFileDialog->Filter = L"Save Files|*.bin|All files|*.*";
			this->openSaveFileDialog->FileOk += gcnew System::ComponentModel::CancelEventHandler(this, &Form1::openSaveFileDialog_FileOk);
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(17, 72);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(136, 13);
			this->label1->TabIndex = 5;
			this->label1->Text = L"Unpacked Savefile folder : ";
			// 
			// txtSaveDirectory
			// 
			this->txtSaveDirectory->Location = System::Drawing::Point(156, 68);
			this->txtSaveDirectory->Name = L"txtSaveDirectory";
			this->txtSaveDirectory->Size = System::Drawing::Size(301, 20);
			this->txtSaveDirectory->TabIndex = 6;
			// 
			// btnOpenSaveFolder
			// 
			this->btnOpenSaveFolder->Location = System::Drawing::Point(463, 66);
			this->btnOpenSaveFolder->Name = L"btnOpenSaveFolder";
			this->btnOpenSaveFolder->Size = System::Drawing::Size(33, 25);
			this->btnOpenSaveFolder->TabIndex = 7;
			this->btnOpenSaveFolder->Text = L"...";
			this->btnOpenSaveFolder->UseVisualStyleBackColor = true;
			this->btnOpenSaveFolder->Click += gcnew System::EventHandler(this, &Form1::btnOpenSaveFolder_Click);
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(509, 66);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(96, 25);
			this->button1->TabIndex = 8;
			this->button1->Text = L"Pack";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &Form1::button1_Click_1);
			// 
			// chkUseBannerBin
			// 
			this->chkUseBannerBin->AutoSize = true;
			this->chkUseBannerBin->Location = System::Drawing::Point(156, 12);
			this->chkUseBannerBin->Name = L"chkUseBannerBin";
			this->chkUseBannerBin->Size = System::Drawing::Size(320, 17);
			this->chkUseBannerBin->TabIndex = 9;
			this->chkUseBannerBin->Text = L"Use banner.bin instead of individual pgm files when unpacking";
			this->chkUseBannerBin->UseVisualStyleBackColor = true;
			// 
			// Form1
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(635, 158);
			this->Controls->Add(this->chkUseBannerBin);
			this->Controls->Add(this->button1);
			this->Controls->Add(this->btnOpenSaveFolder);
			this->Controls->Add(this->txtSaveDirectory);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->btnBrowse);
			this->Controls->Add(this->button2);
			this->Controls->Add(this->btnUnpack);
			this->Controls->Add(this->lblFileName);
			this->Controls->Add(this->txtDataFile);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->Name = L"Form1";
			this->Text = L"FE100 0.3 / By WiiCrazy (I.R.on) ";
			this->Load += gcnew System::EventHandler(this, &Form1::Form1_Load);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) {
				 if(txtDataFile->Text->Equals("")) {
					 MessageBox::Show("Select the savefile before unpacking!","Error",MessageBoxButtons::OK,MessageBoxIcon::Exclamation);
					 return;
				 }
				char* dataFile = (char*)(void*)Marshal::StringToHGlobalAnsi(txtDataFile->Text);
				char* appFolder = (char*)(void*)Marshal::StringToHGlobalAnsi(this->appDomain->BaseDirectory); 

				int returnCode = tachtig(chkUseBannerBin->Checked ? 1 : 0, appFolder, dataFile);

				if (returnCode == 0) 
				{
					MessageBox::Show("Successfully unpacked savefile!", "Information", MessageBoxButtons::OK, MessageBoxIcon::Information);
				} else 
				{
					char* errorStr = get_savelib_errstr();
					String ^ errorStrN = gcnew String(errorStr);
					
					MessageBox::Show("Unpacking failed : " + errorStrN,"Error",MessageBoxButtons::OK,MessageBoxIcon::Stop);
				}

				//Marshal::FreeHGlobal(str2);
				// tachtig(txtDataFile->Text);
			 }

	private: System::Void button2_Click(System::Object^  sender, System::EventArgs^  e) {		 
				 this->frm = (gcnew FE100::InfoForm(this->appDomain));
				 frm->ShowDialog(this);
			 }
private: System::Void Form1_Load(System::Object^  sender, System::EventArgs^  e) {
			 MessageBox::Show("Remember! I (WiiCrazy) as the writer of this software am not responsible for any damage you get using the save files packed with this program, please take preventive measures before you use it. ", "Disclaimer");
		 }
private: System::Void btnBrowse_Click(System::Object^  sender, System::EventArgs^  e) {
	 // Display the openFile Dialog.
	 System::Windows::Forms::DialogResult result = openSaveFileDialog->ShowDialog();	 
     // OK button was pressed.
     if ( result == ::DialogResult::OK )
     {
        try
        {
			//txtDataFile->Text = String::Concat( openSaveFileDlg->SelectedPath  )
			txtDataFile->Text = openSaveFileDialog->FileName;
        }
        catch ( Exception^ exp ) 
        {
			MessageBox::Show( String::Concat( "An error occurred while attempting to locate the file. The error is: ", System::Environment::NewLine, exp, System::Environment::NewLine ), "Error", MessageBoxButtons::OK,MessageBoxIcon::Error );
        }
	 }
     Invalidate();
 }

private: System::Void openSaveFileDialog_FileOk(System::Object^  sender, System::ComponentModel::CancelEventArgs^  e) {
		 }
private: System::Void btnOpenSaveFolder_Click(System::Object^  sender, System::EventArgs^  e) {
	 // Display the open folder Dialog.
	System::Windows::Forms::DialogResult result = openSaveFileDlg->ShowDialog();
     // OK button was pressed.
     if ( result == ::DialogResult::OK )
     {
        try
        {
			//txtDataFile->Text = String::Concat( openSaveFileDlg->SelectedPath  )
			txtSaveDirectory->Text = openSaveFileDlg->SelectedPath;
        }
        catch ( Exception^ exp ) 
        {
			MessageBox::Show( String::Concat( "An error occurred while attempting to locate the folder. The error is: ", System::Environment::NewLine, exp, System::Environment::NewLine ), "Error", MessageBoxButtons::OK, MessageBoxIcon::Error );
        }
	 }
     Invalidate();
		 }
private: System::Void button1_Click_1(System::Object^  sender, System::EventArgs^  e) {
			 if (txtSaveDirectory->Text->Equals("")) {
				 MessageBox::Show("Please select the folder where the unpacked contents of savefile resides!","Error",MessageBoxButtons::OK,MessageBoxIcon::Exclamation);
				 return;
			 } else {
				String^ titleId;
				if ((txtSaveDirectory->Text->Length>16) ) {
					titleId = txtSaveDirectory->Text->Substring(txtSaveDirectory->Text->Length-16, 16);
					if (!System::Text::RegularExpressions::Regex::IsMatch(titleId, "[0-9a-fA-F]{16}")) 
					{
						MessageBox::Show("Folder doesn't seem to be a title Id (16 digits)!","Error",MessageBoxButtons::OK,MessageBoxIcon::Exclamation);
						return;
					}
				} else 
				{
				 MessageBox::Show("Folder doesn't seem to be having unpacked contents of a savefile!","Error",MessageBoxButtons::OK,MessageBoxIcon::Exclamation);
				 return;
				}

				char* sourceDirectory = (char*)(void*)Marshal::StringToHGlobalAnsi(txtSaveDirectory->Text);
				char* sourceFolder = (char*)(void*)Marshal::StringToHGlobalAnsi(titleId);
				char* appFolder = (char*)(void*)Marshal::StringToHGlobalAnsi(this->appDomain->BaseDirectory); 
				int returnCode = twintig(chkUseBannerBin->Checked ? 1 : 0,appFolder, sourceDirectory, sourceFolder, "data.bin");

				if (returnCode == 0) 
				{
					MessageBox::Show("Successfully packed savefile!","Information", MessageBoxButtons::OK, MessageBoxIcon::Information);
				}
				else 
				{
					char* errorStr = get_savelib_errstr();
					String ^ errorStrN = gcnew String(errorStr);
					MessageBox::Show("Packing failed : " + errorStrN, "Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
				}

			 }
		 }
};
}

