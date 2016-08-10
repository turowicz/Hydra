## Hydra
 
 [![NuGet](https://img.shields.io/nuget/v/Mailcloud.Hydra.Core.svg)](https://www.nuget.org/packages/Mailcloud.Hydra.Core/)  [![NuGet](https://img.shields.io/nuget/v/Mailcloud.Hydra.Events.svg)](https://www.nuget.org/packages/Mailcloud.Hydra.Events/) [![Join the chat at https://gitter.im/Mailcloud/Hydra](https://badges.gitter.im/Join%20Chat.svg)](https://gitter.im/Mailcloud/Hydra?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

A set of components to take the most advantage of performance and capacity of Azure Storage. 

Hydra is Azure Subscription agnostic, which means it is possible to use Storage Accounts from different Azure Subscriptions. This functionality gives the developer configurable IOPS and Disk Space with no upper limits.

## Overview

![Link](https://github.com/Mailcloud/Hydra/blob/master/doc/architecture.png)

## Hydra.Core

` class Hydra : IHydra `

A central component for scaling across multiple Storage Accounts. It is using an ISharding strategy to compute consistent hashes that pick a right Storage Account by key provided.

` class JumpSharding : ISharding `

Default implementation of ISharding provided is JumpSharding that implement's Jump Consistent Hash.

### Disclaimer

Hydra.Core doesn't manage shard migration, which means you are constrained the amount of Storage Accounts you start of with. The more the better.

### Advanced usage

It is possible to have multiple instances of Hydra, configured to point at different and/or the same Storage Accounts, with different and/or the same ISharding implementations. That feature gives the developer maximum flexibility for making sure the right data is distributed in the right way.

### Example

Example usage can be found in the Hydra.Tests.Integration namespace.

## Hydra.Events

` class StreamContainer : IStreamContainer `

A central component for managing Stream's underlying storage. It requires an IHydra component to gain access to the storage.

` class Stream : IStream `

This component is in charge of writing and reading events to a stream in storage.

### Disclaimer

Hydra.Events has a limitation dictated by Azure Storage. Currently one stream can consist of up to 50,000 events and 195GB of space.

### Example

Example usage can be found in the Hydra.Tests.Integration namespace.
