//#include "StdAfxCLR.h"
#include "AdeEnrichmentPlugin.h"
#include "DotNetHelper.h"
#using "System.Xml.dll"

#ifdef _DEBUG
  #define new DEBUG_NEW
  #undef THIS_FILE
  static char THIS_FILE[] = __FILE__;
#endif

using namespace System;


/****************************
* Class cDotNetHelper
*****************************/
String^ cDotNetHelper::ToString(std::wstring str)
{
  System::String^ systemStr = gcnew System::String(str.c_str());
  return(systemStr);
}

String^ cDotNetHelper::ToString(double d)
{
  return Xml::XmlConvert::ToString(d);
}

String^ cDotNetHelper::ToString(int d)
{
  return Xml::XmlConvert::ToString(d);
}

String^ cDotNetHelper::ToString(size_t d)
{
  return Xml::XmlConvert::ToString(d);
}

String^ cDotNetHelper::ToString(double d, int precision)
{
  double   h = 5.0;
  int      i, index, length;
  String ^ STR;

  return Xml::XmlConvert::ToString(d);

  for (i = 0; i < precision + 1; i++)
    h = h*0.1;
  d = d + h;

  STR = Xml::XmlConvert::ToString(d);
  index = STR->IndexOf(".");
  if (index == -1)
    return STR;

  length = STR->Length;
  if (index + precision + 1 > length) return STR;
  else return STR->Substring(0, index + precision + 1);
}

String^ cDotNetHelper::ToString(const IfcDB::Point& point, int precision)
{
  System::String ^ STR;

  if (point.dim == 3)
  {
    STR = String::Concat(cDotNetHelper::ToString(point.x, precision), _T(" "),
                         cDotNetHelper::ToString(point.y, precision), _T(" "),
                         cDotNetHelper::ToString(point.z, precision));
  }
  else
  {
    STR = String::Concat(cDotNetHelper::ToString(point.x, precision), _T(" "),
                         cDotNetHelper::ToString(point.y, precision));
  }

  return STR;
}

std::wstring cDotNetHelper::FromString(System::String^ fromStr)
{
  std::wstring toStr;

  if (fromStr)
  {
#ifdef _UNICODE
    IntPtr intPtr = Runtime::InteropServices::Marshal::StringToHGlobalUni(fromStr);
#else
    IntPtr intPtr = Runtime::InteropServices::Marshal::StringToHGlobalAnsi(fromStr);
#endif

    toStr = (LPCTSTR)intPtr.ToPointer();

    Runtime::InteropServices::Marshal::FreeCoTaskMem(intPtr);
  }

  return toStr;
}

void cDotNetHelper::FromString(std::wstring& toStr, System::String^ fromStr)
{
  if (!fromStr)
  {
    toStr.clear();
    return;
  }

#ifdef _UNICODE
  IntPtr intPtr = Runtime::InteropServices::Marshal::StringToHGlobalUni(fromStr);
#else
  IntPtr intPtr = Marshal::StringToHGlobalAnsi(fromStr);
#endif

  toStr = (LPCTSTR)intPtr.ToPointer();

  Runtime::InteropServices::Marshal::FreeCoTaskMem(intPtr);
}

double cDotNetHelper::ToDouble(System::String^ str)
{
  try
  {
    return Convert::ToDouble(str);
  }
  catch (FormatException^)
  {
    return 0.0;
  }
  catch (OverflowException^)
  {
    return 0.0;
  }
}

Guid cDotNetHelper::ToGuid(GUID& guid)
{
  return Guid(guid.Data1, guid.Data2, guid.Data3,
    guid.Data4[0], guid.Data4[1],
    guid.Data4[2], guid.Data4[3],
    guid.Data4[4], guid.Data4[5],
    guid.Data4[6], guid.Data4[7]);
}

std::wstring cDotNetHelper::FromGuid(System::Guid guid)
{
  return FromString(guid.ToString());
}

GUID cDotNetHelper::createGuid()
{
  GUID guid;
  const int len = 100;
  TCHAR guidString[len];
  std::wstring guidStr = IfcDB::CreateCompressedGuidString(guidString, len);
  IfcDB::GetGuidFromString64(guidStr.c_str(), &guid);

  return guid;
}

int64_t cDotNetHelper::DateTime2Seconds(System::DateTime date)
{
  System::TimeSpan diff = date.ToUniversalTime() - System::DateTime(1970, 1, 1);
  return (int64_t)diff.TotalSeconds;
}

System::DateTime cDotNetHelper::time_t2DateTime(std::time_t date)
{
  double sec = static_cast<double>(date);
  return System::DateTime(1970, 1, 1, 0, 0, 0, System::DateTimeKind::Utc).AddSeconds(sec);
}

std::time_t cDotNetHelper::DateTime2time_t(System::DateTime date)
{
  System::TimeSpan diff = date.ToUniversalTime() - System::DateTime(1970, 1, 1);
  return static_cast<std::time_t>(diff.TotalSeconds);
}

System::TimeZoneInfo^ cDotNetHelper::getTimeZoneInfo(System::String^ windowsTimeZoneId)
{
  try
  {
    System::TimeZoneInfo^ windowsTimeZone = System::TimeZoneInfo::FindSystemTimeZoneById(windowsTimeZoneId);

    return windowsTimeZone;
  }
  catch (TimeZoneNotFoundException^)
  {
    return nullptr;
  }
  catch (InvalidTimeZoneException^)
  {
    return nullptr;
  }

  return nullptr;
}

bool cDotNetHelper::getTimeZoneOffset(System::String^ windowsTimeZoneId, double& timeZoneOffset)
{
  bool state(false);

  System::TimeZoneInfo^ timeZoneInfo = cDotNetHelper::getTimeZoneInfo(windowsTimeZoneId);

  if (timeZoneInfo != nullptr)
  {
    timeZoneOffset = timeZoneInfo->BaseUtcOffset.Hours + timeZoneInfo->BaseUtcOffset.Minutes / 60;
    state = true;
  }

  return state;
}
