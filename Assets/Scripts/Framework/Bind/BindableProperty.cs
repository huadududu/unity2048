using System;

public class BindableProperty<T> where T : IEquatable<T>
{
    private T mValue = default(T);
    public T Value
    {
        get => mValue;
        set
        {
            if (!value.Equals(mValue))
            {
                mValue = value;
                OnValueChanged?.Invoke(mValue);
            }
        }
    }

    public event Action<T> OnValueChanged;
}
