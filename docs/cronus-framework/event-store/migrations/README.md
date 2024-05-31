# Migrations

## What is data migration?

Data migration is the process of moving data from one system to another. And there are many reasons why a system may require such a move. To name most common ones:

* Natural system evolution which requires the data to be optimized for performance or maintainability.
* Legal issues where some parts of the data have to be deleted or encrypted
* Bad data created by a bug in the system
* Business reason. When businesses merge or split.

{% hint style="warning" %}
It is important that the business value of the data is not changed during the process.
{% endhint %}

There are many different strategies when and how to do data migration. You must carefully plan and execute because damages could be significant.

## Challenges

Depending on the data volume the migration process could take hours, even days. During that time there are many things which could fail and corrupt the data in a irreversible way. To avoid such scenarios you should always migrate the data into a new storage repository.

{% hint style="info" %}
Always migrate the data into a new storage repository.
{% endhint %}

Make sure the migration process does not overwhelm the live system. You should be in control when the data is being migrated so you could pause the migration during peek times of the live system. To achieve this, use a separate process to run data migration. Always keep in mind that migrating data takes from your system resources and you must account for that.

{% hint style="info" %}
Use a separate process to run data migration.
{% endhint %}

When you are migrating a&#x20;

## How to do

1. Create a separate process which migrates the existing data into the new data repository
2. Live system must push any new data to the migration service. Could be easily achieved by sending it to a message broker.
