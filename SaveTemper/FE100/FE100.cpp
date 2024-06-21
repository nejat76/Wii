// FE100.cpp : main project file.
#pragma unmanaged 
#include "stdafx.h"
#include "Form1.h"

#include "savetools.h"

using namespace FE100;

using namespace System;
using namespace System::Reflection;
using namespace System::Security::Policy;

[STAThreadAttribute]
int main(array<System::String ^> ^args)
{
	// Enabling Windows XP visual effects before any controls are created
	Application::EnableVisualStyles();
	Application::SetCompatibleTextRenderingDefault(false); 

	#pragma endregion

	   // Create application domain setup information
   AppDomainSetup^ domaininfo = gcnew AppDomainSetup;
   domaininfo->ApplicationBase = String::Format( "{0}", System::Environment::CurrentDirectory );

   //Create evidence for the new appdomain from evidence of the current application domain
   Evidence^ adevidence = AppDomain::CurrentDomain->Evidence;

   // Create appdomain
   AppDomain^ domain = AppDomain::CreateDomain( "MyDomain", adevidence, domaininfo );

   // Write out application domain information
   Console::WriteLine( "Host domain: {0}", AppDomain::CurrentDomain->FriendlyName );
   Console::WriteLine( "child domain: {0}", domain->FriendlyName );
   Console::WriteLine();
   Console::WriteLine( "Application Base Directory is: {0}", domain->BaseDirectory );


	// Create the main window and run it
	Application::Run(gcnew Form1(domain));
	return 0;
}
