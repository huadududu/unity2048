
public class Singleton<T> where T : Singleton<T>
{
    private static T mInstance; 
    public static T Instance
    {
        get
        {
            if (mInstance == null)
            {
                var type = typeof(T);
                var ctors = type.GetConstructors(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                var ctor = System.Array.Find(ctors, c => c.GetParameters().Length == 0);
                if (ctor == null)
                {
                    throw new System.Exception($"Non-public ctor() not found! {type}");
                }
                mInstance = (T)ctor.Invoke(null);
            }
            return mInstance;
        }
    }   
}
