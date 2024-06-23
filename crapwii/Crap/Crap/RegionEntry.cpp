using namespace System;

namespace FE100 {

	public class RegionEntry
	{
	
	public : 
		String^ code;
		String^ name;

		virtual String^ ToString() override
		{
			return name;
		}

	}
}