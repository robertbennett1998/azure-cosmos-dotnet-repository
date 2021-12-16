﻿// Copyright (c) IEvangelist. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using Newtonsoft.Json;

namespace Microsoft.Azure.CosmosRepository
{
    /// <summary>
    /// A base helper class that implements IItemWithEtag
    /// </summary>
    /// <example>
    /// Here is an example subclass item, which adds several properties:
    /// <code language="c#">
    /// <![CDATA[
    /// public class SubItem : EtagItem
    /// {
    ///     public DateTimeOffset Date { get; set; }
    ///     public string Name { get; set; }
    ///     public IEnumerable<Child> Children { get; set; }
    ///     public IEnumerable<string> Tags { get; set; }
    /// }
    ///
    /// public class Child
    /// {
    ///     public string Name { get; set; }
    ///     public DateTime BirthDate { get; set; }
    /// }
    /// ]]>
    /// </code>
    /// </example>
    public class EtagItem : IItemWithEtag
    {
        /// <summary>
        /// Gets or sets the item's globally unique identifier.
        /// </summary>
        /// <remarks>
        /// Initialized by <see cref="Guid.NewGuid"/>.
        /// </remarks>
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets or sets the item's type name. This is used as a discriminator.
        /// </summary>
        [JsonProperty("type")]
        public string Type { get; set; }

        /// <summary>
        /// Etag for the item which was set by Cosmos the last time the item was updated. This string is used for the relevant operations when specified.
        /// </summary>
        /// <remarks>
        /// Will only be used if the verifyEtag flag is specified on the relevant methods.
        /// </remarks>
        [JsonProperty("_etag")]
        public string Etag { get; set; }

        /// <summary>
        /// Gets the PartitionKey based on <see cref="GetPartitionKeyValue"/>.
        /// Implemented explicitly to keep out of Item API
        /// </summary>
        string IItem.PartitionKey => GetPartitionKeyValue();

        /// <summary>
        /// Default constructor, assigns type name to <see cref="Type"/> property.
        /// </summary>
        public EtagItem() => Type = GetType().Name;

        /// <summary>
        /// Gets the partition key value for the given <see cref="Item"/> type.
        /// When overridden, be sure that the <see cref="Path"/> value corresponds
        /// to the <see cref="JsonPropertyAttribute.PropertyName"/> value, i.e.; "/partition" and "partition"
        /// respectively. If these two values do not correspond an error will occur.
        /// </summary>
        /// <returns>The <see cref="Item.Id"/> unless overridden by the subclass.</returns>
        protected virtual string GetPartitionKeyValue() => Id;
    }
}
