# Copy EventStore

## **Issue at hand**

An issue that came up in the past was that we serialized a huge amount of information in an event. The event contained a structure that in itself had a very innocent looking property called `TimeZoneInfo`:
```c#
    [DataContract(Namespace = BC.ContextName, Name = "dce741fb-8671-42b8-af59-d30aaae27bad")]
    public struct Cycle
    {
        [DataMember(Order = 1)]
        private DateTimeOffset _start;

        [DataMember(Order = 2)]
        private DateTimeOffset _end;

        [DataMember(Order = 3)]
        private TimeSpan _duration;

        [DataMember(Order = 4)]
        private readonly TimeZoneInfo _timezone;
    }
```

After releasing the software, we noticed that the project was taking up an unusually large amount of space. After checking out a couple of persisted events, we found out that each time we used the struct `Cycle`, we persisted some 6200 lines of serialized json.  Of which, the 6000 lines were attributed to the `TimeZoneInfo`.This severly impacted event serialization and deserialization. The issue came up after we had doing the following assignment
```c#
{
    ...
    _timezone = TZConvert.GetTimeZoneInfo("Central Standard Time");
    ...
}
```

## **Decision**

We decided that in order to lower the amount of data, we needed to migrate the event store while keeping up a live version of the old one, to avoid downtime.

## **Migration Challenges**

In order to avoid having downtime, we decided to create a single deployable service (let's call it Migrator) which subscribed to the same events as the original application service. However, the Migrator would write the events directly in the new event store. Furthermore, the Migrator would be responsible for once it boots, to start copying data over from the old event store while applying the needed changes. In our case, we needed to modify all events that had the `Cycle` in them, and replace the `TimeZoneInfo` with just a `TimeZoneId` which is a simple string.

## **How To Do this**

### Changing the structure

We changed the structure of the Cycle to this:

```c#
    [DataContract(Namespace = BC.ContextName, Name = "dce741fb-8671-42b8-af59-d30aaae27bad")]
    public struct Cycle
    {
        [DataMember(Order = 1)]
        private DateTimeOffset _start;

        [DataMember(Order = 2)]
        private DateTimeOffset _end;

        [DataMember(Order = 3)]
        private TimeSpan _duration;

        [DataMember(Order = 4)]
        private readonly string _timezoneId;
    }
```

### Creating the project

