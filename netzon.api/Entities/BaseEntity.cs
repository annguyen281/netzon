namespace Netzon.Api.Entities
{
    public abstract partial class BaseEntity
    {
        public int Id { get; set; }

        public override int GetHashCode()
        {
            if (Equals(Id, default(int)))
                return base.GetHashCode();
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BaseEntity);
        }

        public virtual bool Equals(BaseEntity other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return false;
        }

        public static bool operator ==(BaseEntity x, BaseEntity y) => Equals(x, y);

        public static bool operator !=(BaseEntity x, BaseEntity y)
        {
            return !(x == y);
        }
    }
}