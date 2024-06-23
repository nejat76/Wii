//Crap, Copyright 2009 WiiCrazy/I.R.on of Irduco (nejat@tepetaklak.com)
//Distributed under the terms of the GNU GPL v2.0
//See http://www.gnu.org/licenses/old-licenses/gpl-2.0.txt for more information

#pragma once

using namespace System;
using namespace System::ComponentModel;
using namespace System::Collections;
using namespace System::Windows::Forms;
using namespace System::Data;
using namespace System::Drawing;


namespace FE100 {

	/// <summary>
	/// Summary for InfoForm
	///
	/// WARNING: If you change the name of this class, you will need to change the
	///          'Resource File Name' property for the managed resource compiler tool
	///          associated with all .resx files this class depends on.  Otherwise,
	///          the designers will not be able to interact properly with localized
	///          resources associated with this form.
	/// </summary>
	public ref class InfoForm : public System::Windows::Forms::Form
	{
	public:
		InfoForm(System::AppDomain^ appDomain)
		{
			this->appDomain = appDomain;
			InitializeComponent();
			InitializeMyComponent();
			//
			//TODO: Add the constructor code here
			//
		}

	protected:
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		~InfoForm()
		{
			if (components)
			{
				delete components;
			}
		}
	private: System::Windows::Forms::TextBox^  txtInfo;
	//private: HWAVEOUT * waveOut;
	private: System::AppDomain^ appDomain;
	private: System::Collections::ArrayList^ listZak;
	private: System::Collections::ArrayList^ nameListZak;	
	private: System::Windows::Forms::Label^  lblZakCredits;
	private: System::Windows::Forms::Label^  label1;

	protected: 

	private:
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System::ComponentModel::Container ^components;

		void InitializeMyComponent(void)
		{
			listZak = gcnew System::Collections::ArrayList();
			listZak->Add("jt_breez");//added 3 times to make it more probable in random
			listZak->Add("jt_breez");
			listZak->Add("jt_breez");
			listZak->Add("jt_mind");//added 4 times to make it much more probable in random
			listZak->Add("jt_mind");
			listZak->Add("jt_mind");
			listZak->Add("jt_mind");
			listZak->Add("jt_1999");
			listZak->Add("jt_letgo");
			listZak->Add("jt_xmas");

			nameListZak = gcnew System::Collections::ArrayList();
			String^ zak1 = "Mountain Breeze by Jeroen Tel";
			String^ zak2 = "In my life, in my mind by Jeroen Tel";
			nameListZak->Add(zak1);nameListZak->Add(zak1);nameListZak->Add(zak1);
			nameListZak->Add(zak2);nameListZak->Add(zak2);nameListZak->Add(zak2);nameListZak->Add(zak2);
			nameListZak->Add("1999 by Jeroen Tel");
			nameListZak->Add("Letting go by Jeroen Tel");
			nameListZak->Add("Merry Xmas & Funky99 by Jeroen Tel");
		}

#pragma region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent(void)
		{
			System::ComponentModel::ComponentResourceManager^  resources = (gcnew System::ComponentModel::ComponentResourceManager(InfoForm::typeid));
			this->txtInfo = (gcnew System::Windows::Forms::TextBox());
			this->lblZakCredits = (gcnew System::Windows::Forms::Label());
			this->label1 = (gcnew System::Windows::Forms::Label());
			this->SuspendLayout();
			// 
			// txtInfo
			// 
			this->txtInfo->Font = (gcnew System::Drawing::Font(L"Courier New", 9.75F, System::Drawing::FontStyle::Regular, System::Drawing::GraphicsUnit::Point, 
				static_cast<System::Byte>(162)));
			this->txtInfo->Location = System::Drawing::Point(8, 10);
			this->txtInfo->Multiline = true;
			this->txtInfo->Name = L"txtInfo";
			this->txtInfo->ReadOnly = true;
			this->txtInfo->ScrollBars = System::Windows::Forms::ScrollBars::Vertical;
			this->txtInfo->Size = System::Drawing::Size(597, 482);
			this->txtInfo->TabIndex = 1;
			this->txtInfo->Text = resources->GetString(L"txtInfo.Text");
			this->txtInfo->TextChanged += gcnew System::EventHandler(this, &InfoForm::txtInfo_TextChanged);
			// 
			// lblZakCredits
			// 
			this->lblZakCredits->AutoSize = true;
			this->lblZakCredits->Font = (gcnew System::Drawing::Font(L"Microsoft Sans Serif", 8.25F, static_cast<System::Drawing::FontStyle>((System::Drawing::FontStyle::Bold | System::Drawing::FontStyle::Italic)), 
				System::Drawing::GraphicsUnit::Point, static_cast<System::Byte>(162)));
			this->lblZakCredits->Location = System::Drawing::Point(115, 505);
			this->lblZakCredits->Name = L"lblZakCredits";
			this->lblZakCredits->Size = System::Drawing::Size(73, 13);
			this->lblZakCredits->TabIndex = 1;
			this->lblZakCredits->Text = L"labelCredits";
			// 
			// label1
			// 
			this->label1->AutoSize = true;
			this->label1->Location = System::Drawing::Point(18, 505);
			this->label1->Name = L"label1";
			this->label1->Size = System::Drawing::Size(97, 13);
			this->label1->TabIndex = 2;
			this->label1->Text = L"You are listening to";
			// 
			// InfoForm
			// 
			this->AutoScaleDimensions = System::Drawing::SizeF(6, 13);
			this->AutoScaleMode = System::Windows::Forms::AutoScaleMode::Font;
			this->ClientSize = System::Drawing::Size(614, 533);
			this->Controls->Add(this->label1);
			this->Controls->Add(this->lblZakCredits);
			this->Controls->Add(this->txtInfo);
			this->Icon = (cli::safe_cast<System::Drawing::Icon^  >(resources->GetObject(L"$this.Icon")));
			this->MaximizeBox = false;
			this->MinimizeBox = false;
			this->Name = L"InfoForm";
			this->SizeGripStyle = System::Windows::Forms::SizeGripStyle::Hide;
			this->StartPosition = System::Windows::Forms::FormStartPosition::CenterScreen;
			this->Text = L"Info....";
			this->Deactivate += gcnew System::EventHandler(this, &InfoForm::InfoForm_Deactivate);
			this->Load += gcnew System::EventHandler(this, &InfoForm::InfoForm_Load);
			this->FormClosed += gcnew System::Windows::Forms::FormClosedEventHandler(this, &InfoForm::InfoForm_FormClosed);
			this->ResumeLayout(false);
			this->PerformLayout();

		}
#pragma endregion
	private: System::Void InfoForm_Deactivate(System::Object^  sender, System::EventArgs^  e) {
			 }
	private: System::Void InfoForm_Load(System::Object^  sender, System::EventArgs^  e) {
				 //Randomly play a tune on form load....
				 System::Random^ rnd = gcnew System::Random();
				 int randomZak = rnd->Next(listZak->Count);
				 String^ strFileToPlay = this->appDomain->BaseDirectory + "\\Muzak\\" + listZak[randomZak] + ".xm";
				 lblZakCredits->Text = nameListZak[randomZak]->ToString();

				 //TODO: Convert to C#
				 //char* ptrStr = (char*)(void*)Marshal::StringToHGlobalAnsi(strFileToPlay);
				 //this->waveOut = uFMOD_PlaySong(ptrStr ,"0",XM_FILE);	
			 }
	private: System::Void InfoForm_FormClosed(System::Object^  sender, System::Windows::Forms::FormClosedEventArgs^  e) {
				 //TODO: Convert to C#
				 //uFMOD_StopSong();
			 }
private: System::Void txtInfo_TextChanged(System::Object^  sender, System::EventArgs^  e) {
		 }
};
}

