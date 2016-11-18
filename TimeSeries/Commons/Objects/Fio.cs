using System;
using System.Linq;

using JetBrains.Annotations;

namespace SKBKontur.Catalogue.Objects
{
    public class Fio : IEquatable<Fio>
    {
        public Fio([CanBeNull] string surname, [CanBeNull] string firstName, [CanBeNull] string patronymic)
        {
            Surname = surname;
            FirstName = firstName;
            Patronymic = patronymic;
        }

        [CanBeNull]
        public string Surname { get; private set; }

        [CanBeNull]
        public string FirstName { get; private set; }

        [CanBeNull]
        public string Patronymic { get; private set; }

        [NotNull]
        public string GetCombinedName()
        {
            return string.Join(" ", new[] {Surname, FirstName, Patronymic}.Where(s => !string.IsNullOrEmpty(s)));
        }

        public override string ToString()
        {
            return string.Format("Surname: {0}, FirstName: {1}, Patronymic: {2}", Surname, FirstName, Patronymic);
        }

        public bool Equals(Fio other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return string.Equals(Surname, other.Surname) && string.Equals(FirstName, other.FirstName) && string.Equals(Patronymic, other.Patronymic);
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((Fio)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Surname != null ? Surname.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FirstName != null ? FirstName.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Patronymic != null ? Patronymic.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(Fio left, Fio right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Fio left, Fio right)
        {
            return !Equals(left, right);
        }
    }
}