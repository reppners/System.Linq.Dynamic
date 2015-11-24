using FluentValidationNA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.Linq.Dynamic.Tests.Helpers
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; }

        public int Income { get; set; }

        public UserProfile Profile { get; set; }

        public List<Role> Roles { get; set; }

        public UtcDateTime LastLogin { get; set; }

        public static IList<User> GenerateSampleModels(int total, bool allowNullableProfiles = false)
        {
            Validate.Argument(total).IsInRange(x => total >= 0).Check();

            var list = new List<User>();
            var lastloginDate = DateTime.UtcNow;

            for (int i = 0; i < total; i++)
            {
                var user = new User()
                {
                    Id = Guid.NewGuid(),
                    UserName = "User" + i.ToString(),
                    Income = ((i) % 15) * 100,
                    LastLogin = new UtcDateTime(lastloginDate.AddMinutes(i))
                };

                if (!allowNullableProfiles || (i % 8) != 5)
                {
                    user.Profile = new UserProfile()
                    {
                        FirstName = "FirstName" + i,
                        LastName = "LastName" + i,
                        Age = (i % 50) + 18
                    };
                }

                user.Roles = new List<Role>(Role.StandardRoles);

                list.Add(user);
            }

            return list.ToArray();
        }
    }

    public class UserProfile
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public int? Age { get; set; }
    }

    public class Role
    {
        public static readonly Role[] StandardRoles = new Role[] {
            new Role() { Name="Admin"},
            new Role() { Name="User"},
            new Role() { Name="Guest"},
            new Role() { Name="G"},
            new Role() { Name="J"},
            new Role() { Name="A"},
        };

        public Role()
        {
            Id = Guid.NewGuid();
        }

        public Guid Id { get; set; }

        public string Name { get; set; }
    }

    [DynamicLinqType]
    public class UtcDateTime : IComparable<UtcDateTime>, IComparable<DateTime>
    {
        private readonly DateTime utcDateTime;

        public static UtcDateTime Now
        {
            get
            {
                return new UtcDateTime(DateTime.Now);
            }
        }

        public UtcDateTime(UtcDateTime other)
        {
            this.utcDateTime = other.utcDateTime;
        }

        public UtcDateTime(int year, int month, int day, int hour, int minute, int second)
            : this(new DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc))
        {
            // Nothing
        }

        public UtcDateTime(DateTime dateTime)
        {
            switch (dateTime.Kind)
            {
                case DateTimeKind.Local:
                    this.utcDateTime = dateTime.ToUniversalTime();
                    break;

                case DateTimeKind.Utc:
                    this.utcDateTime = dateTime;
                    break;

                case DateTimeKind.Unspecified:
                    throw new ArgumentException("Unspecified time zone", "dateTime");

                default:
                    throw new ArgumentException("Unknown value of property Kind", "dateTime");
            }
        }

        public DateTime AsUtc()
        {
            return this.utcDateTime;
        }

        public DateTime AsLocalTime()
        {
            return this.utcDateTime.ToLocalTime();
        }

        #region Equals & HashCode

        /// <see cref="Object.Equals(Object)"/>
        public override bool Equals(Object obj)
        {
            // Check for identity
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj == null)
            {
                return false;
            }

            // Check for equality
            if (obj is UtcDateTime)
            {
                UtcDateTime other = (UtcDateTime)obj;

                bool result = Object.Equals(this.utcDateTime, other.utcDateTime);
                return result;
            }

            if (obj is DateTime)
            {
                DateTime other = (DateTime)obj;

                bool result = this.Equals(new UtcDateTime(other));
                return result;
            }

            return false;
        }

        /// <see cref="Object.GetHashCode()"/>
        public override int GetHashCode()
        {
            int result = this.utcDateTime.GetHashCode();
            return result;
        }

        #endregion

        #region IComparable

        public int CompareTo(UtcDateTime other)
        {
            if (this.utcDateTime < other.utcDateTime)
            {
                return -1;
            }

            if (this.utcDateTime > other.utcDateTime)
            {
                return 1;
            }

            return 0;
        }

        public int CompareTo(DateTime other)
        {
            return this.CompareTo(new UtcDateTime(other));
        }

        #endregion

        #region Static Compare

        public static int Compare(UtcDateTime a, UtcDateTime b)
        {
            return DateTime.Compare(a.AsUtc(), b.AsUtc());
        }

        #endregion

        #region Operators

        #region UtcDateTime - UtcDateTime

        public static bool operator ==(UtcDateTime a, UtcDateTime b)
        {
            if (Object.ReferenceEquals(a, null) && Object.ReferenceEquals(b, null))
            {
                return true;
            }

            if (Object.ReferenceEquals(a, null) || Object.ReferenceEquals(b, null))
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(UtcDateTime a, UtcDateTime b)
        {
            return !(a == b);
        }

        public static bool operator <(UtcDateTime a, UtcDateTime b)
        {
            // ReSharper disable once PossibleNullReferenceException
            return a.CompareTo(b) == -1;
        }

        public static bool operator >(UtcDateTime a, UtcDateTime b)
        {
            // ReSharper disable once PossibleNullReferenceException
            return a.CompareTo(b) == 1;
        }

        public static bool operator <=(UtcDateTime a, UtcDateTime b)
        {
            // ReSharper disable once PossibleNullReferenceException
            int compareValue = a.CompareTo(b);
            return compareValue == -1 || compareValue == 0;
        }

        public static bool operator >=(UtcDateTime a, UtcDateTime b)
        {
            // ReSharper disable once PossibleNullReferenceException
            int compareValue = a.CompareTo(b);
            return compareValue == 1 || compareValue == 0;
        }

        #endregion

        #region UtcDateTime - DateTime

        public static bool operator ==(UtcDateTime a, DateTime b)
        {
            // b cannot be null, because it is a struct
            // So this means: a == null && b != null => false
            if (Object.ReferenceEquals(a, null))
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(UtcDateTime a, DateTime b)
        {
            return !(a == b);
        }

        public static bool operator <(UtcDateTime a, DateTime b)
        {
            // ReSharper disable once PossibleNullReferenceException
            return a.CompareTo(b) == -1;
        }

        public static bool operator >(UtcDateTime a, DateTime b)
        {
            return a.CompareTo(b) == 1;
        }

        public static bool operator <=(UtcDateTime a, DateTime b)
        {
            int compareValue = a.CompareTo(b);
            return compareValue == -1 || compareValue == 0;
        }

        public static bool operator >=(UtcDateTime a, DateTime b)
        {
            int compareValue = a.CompareTo(b);
            return compareValue == 1 || compareValue == 0;
        }

        #endregion

        #region DateTime - UtcDateTime

        public static bool operator ==(DateTime a, UtcDateTime b)
        {
            // a cannot be null, because it is a struct
            // So this means a != null && b == null => false
            if (Object.ReferenceEquals(b, null))
            {
                return false;
            }

            return a.CompareTo(b) == 0;
        }

        public static bool operator !=(DateTime a, UtcDateTime b)
        {
            return !(a == b);
        }

        public static bool operator <(DateTime a, UtcDateTime b)
        {
            return b.CompareTo(a) == 1;
        }

        public static bool operator >(DateTime a, UtcDateTime b)
        {
            return b.CompareTo(a) == -1;
        }

        public static bool operator <=(DateTime a, UtcDateTime b)
        {
            int compareValue = b.CompareTo(a);
            return compareValue == 1 || compareValue == 0;
        }

        public static bool operator >=(DateTime a, UtcDateTime b)
        {
            int compareValue = b.CompareTo(a);
            return compareValue == -1 || compareValue == 0;
        }

        #endregion

        #endregion
    }
}
