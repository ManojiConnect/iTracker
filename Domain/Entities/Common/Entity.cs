using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Common;

/// <summary>
/// Base entity class that provides common properties and functionality for all domain entities.
/// This class implements basic entity tracking and soft delete functionality.
/// </summary>
public class Entity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    [Key]
    public int Id { get; set; }

    /// <summary>
    /// ID of the user who created this entity.
    /// </summary>
    public int CreatedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was created.
    /// </summary>
    [Precision(0)]
    public DateTime CreatedOn { get; set; }

    /// <summary>
    /// ID of the user who last modified this entity. Null if never modified.
    /// </summary>
    public int? ModifiedBy { get; set; }

    /// <summary>
    /// Timestamp when the entity was last modified. Null if never modified.
    /// </summary>
    [Precision(0)]
    public DateTime? ModifiedOn { get; set; }

    /// <summary>
    /// Indicates whether the entity is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Indicates whether the entity has been soft-deleted.
    /// </summary>
    public bool IsDelete { get; set; } = false;

    /// <summary>
    /// Determines whether the specified object is equal to the current entity.
    /// Entities are considered equal if they are of the same type and have the same Id.
    /// </summary>
    /// <param name="obj">The object to compare with the current entity.</param>
    /// <returns>true if the objects are equal; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        var compareTo = obj as Entity;

        if (ReferenceEquals(this, compareTo)) return true;
        if (compareTo is null) return false;

        return Id.Equals(compareTo.Id);
    }

    /// <summary>
    /// Determines whether two entities are equal.
    /// </summary>
    /// <param name="a">The first entity to compare.</param>
    /// <param name="b">The second entity to compare.</param>
    /// <returns>true if the entities are equal; otherwise, false.</returns>
    public static bool operator ==(Entity? a, Entity? b)
    {
        if (a is null && b is null)
            return true;

        if (a is null || b is null)
            return false;

        return a.Equals(b);
    }

    /// <summary>
    /// Determines whether two entities are not equal.
    /// </summary>
    /// <param name="a">The first entity to compare.</param>
    /// <param name="b">The second entity to compare.</param>
    /// <returns>true if the entities are not equal; otherwise, false.</returns>
    public static bool operator !=(Entity? a, Entity? b)
    {
        return !(a == b);
    }

    /// <summary>
    /// Serves as a hash function for the entity.
    /// </summary>
    /// <returns>A hash code for the current entity.</returns>
    public override int GetHashCode()
    {
        return (GetType().GetHashCode() * 907) + Id.GetHashCode();
    }

    /// <summary>
    /// Returns a string representation of the entity.
    /// </summary>
    /// <returns>A string that represents the current entity.</returns>
    public override string ToString()
    {
        return $"{GetType().Name} [Id={Id}]";
    }
}
