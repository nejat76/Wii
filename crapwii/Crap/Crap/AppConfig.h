#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;
using namespace Org::Irduco::MultiLanguage;
using namespace System::Configuration;

namespace FE100 {

	/// <summary>
	/// Summary for AppConfig
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class AppConfig : public System::Windows::Forms::Form
	{
	public:
		AppConfig(void)
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
		~AppConfig()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  txtIpAddress;
	protected: 

	protected: 
	private: System::Windows::Forms::Label^  label1;
	private: System::Windows::Forms::Button^  button1;
	public: MultiLanguageModuleHelper^ guiLang;

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
			this->txtIpAddress = (gcnew System::Windows::Forms::TextBox());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->button1 = (gcnew System::Windows::Forms::Button());
			this->SuspendLayout();
			// 
			// txtIpAddress
			// 
			this->txtIpAddress->Location = System::Drawing::Point(12, 31);
			this->txtIpAddress->Name = L"txtIpAddress";
			this->txtIpAddress->Size = System::Drawing::Size(172, 20);
			this->txtIpAddress->TabIndex = 0;
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(13, 13);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(110, 13);
			this->label1->TabIndex = 1;
			this->label1->Text = L"Ip Address of your Wii";
			// 
			// button1
			// 
			this->button1->Location = System::Drawing::Point(190, 29);
			this->button1->Name = L"button1";
			this->button1->Size = System::Drawing::Size(75, 23);
			this->button1->TabIndex = 2;
			this->button1->Text = L"Save";
			this->button1->UseVisualStyleBackColor = true;
			this->button1->Click += gcnew System::EventHandler(this, &AppConfig::button1_Click);
			// 
			// AppConfig
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(307, 69);
			this->Controls->Add(this->button1);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->txtIpAddress);
			this->FormBorderStyle = System::Windows::Forms::FormBorderStyle::FixedDialog;
			this->Name = L"AppConfig";
			this->Text = L"Configure";
			this->Load += gcnew System::EventHandler(this, &AppConfig::AppConfig_Load);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void button1_Click(System::Object^  sender, System::EventArgs^  e) 
			 {
				saveConfig();
			 }

	private: void saveConfig()
        {
            try
            {				
				System::Configuration::Configuration^ config = System::Configuration::ConfigurationManager::OpenExeConfiguration(ConfigurationUserLevel::None);
                config->AppSettings->Settings->Remove("IpAddressOfWii");
				config->AppSettings->Settings->Add("IpAddressOfWii", txtIpAddress->Text);
				config->Save(ConfigurationSaveMode::Modified);
				ConfigurationManager::RefreshSection("appSettings");
				this->Close();
            }
            catch (Exception^ ex)
            {
				MessageBox::Show(guiLang->TranslateAndReplace("SAVEERROR", "\\n", "\n") + ": " + ex->Message, "Error", MessageBoxButtons::OK, MessageBoxIcon::Error);
            }

        }

	private: System::Void AppConfig_Load(System::Object^  sender, System::EventArgs^  e) {
			 loadMLResources();
			 txtIpAddress->Text = System::Configuration::ConfigurationSettings::AppSettings["IpAddressOfWii"];
		 }

private: void loadMLResources(){
			this->label1->Text = guiLang->Translate("IPADDRESS");
			this->button1->Text = guiLang->Translate("SAVE");
			this->Text = guiLang->Translate("CONFIGURE");
}

};
}
