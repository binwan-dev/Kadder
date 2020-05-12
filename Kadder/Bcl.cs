using System;
using System.Collections.Generic;

namespace Kadder
{
    internal class Bcl
    {   
    	internal static string Proto=@"message TimeSpan {
  sint64 value = 1; // the size of the timespan (in units of the selected scale)
  TimeSpanScale scale = 2; // the scale of the timespan [default = DAYS]
  enum TimeSpanScale {
    DAYS = 0;
    HOURS = 1;
    MINUTES = 2;
    SECONDS = 3;
    MILLISECONDS = 4;
	TICKS = 5;

    MINMAX = 15; // dubious
  }
}

message DateTime {
  sint64 value = 1; // the offset (in units of the selected scale) from 1970/01/01
  TimeSpanScale scale = 2; // the scale of the timespan [default = DAYS]
  DateTimeKind kind = 3; // the kind of date/time being represented [default = UNSPECIFIED]
  enum TimeSpanScale {
    DAYS = 0;
    HOURS = 1;
    MINUTES = 2;
    SECONDS = 3;
    MILLISECONDS = 4;
	TICKS = 5;

    MINMAX = 15; // dubious
  }
  enum DateTimeKind
  {     
     // The time represented is not specified as either local time or Coordinated Universal Time (UTC).
     UNSPECIFIED = 0;
     // The time represented is UTC.
     UTC = 1;
     // The time represented is local time.
     LOCAL = 2;
   }
}

message NetObjectProxy {
  int32 existingObjectKey = 1; // for a tracked object, the key of the **first** time this object was seen
  int32 newObjectKey = 2; // for a tracked object, a **new** key, the first time this object is seen
  int32 existingTypeKey = 3; // for dynamic typing, the key of the **first** time this type was seen
  int32 newTypeKey = 4; // for dynamic typing, a **new** key, the first time this type is seen
  string typeName = 8; // for dynamic typing, the name of the type (only present along with newTypeKey)
  bytes payload = 10; // the new string/value (only present along with newObjectKey)
}

message Guid {
  fixed64 lo = 1; // the first 8 bytes of the guid (note:crazy-endian)
  fixed64 hi = 2; // the second 8 bytes of the guid (note:crazy-endian)
}

message Decimal {
  uint64 lo = 1; // the first 64 bits of the underlying value
  uint32 hi = 2; // the last 32 bis of the underlying value
  uint32 signScale = 3; // the number of decimal digits (bits 1-16), and the sign (bit 0)
}";
    }
}
