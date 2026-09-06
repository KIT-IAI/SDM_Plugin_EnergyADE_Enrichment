#pragma once

/****************************
* Class cDotNetHelper
*****************************/
class cDotNetHelper
{
  public:
    static System::String^ ToString(std::wstring str);
    static System::String^ ToString(double d);
    static System::String^ ToString(int d);
    static System::String^ ToString(size_t d);
    static System::String^ ToString(double d, int precision);
    static System::String^ ToString(const IfcDB::Point& point, int precision);
    static std::wstring FromString(System::String^ fromStr);
    static void FromString(std::wstring& toStr, System::String^ fromStr);
    static double ToDouble(System::String^ str);
    static System::Guid ToGuid(GUID& guid);
    static std::wstring FromGuid(System::Guid);
    static GUID createGuid();
    static int64_t DateTime2Seconds(System::DateTime date);
    static System::DateTime time_t2DateTime(std::time_t date);
    static std::time_t DateTime2time_t(System::DateTime date);

    static System::TimeZoneInfo^ getTimeZoneInfo(System::String^ windowsTimeZoneId);
    static bool getTimeZoneOffset(System::String^ olsonTimeZoneId, double& timeZoneOffset);
};
