using Newtonsoft.Json;

namespace AccessibilityInsights.Misc
{
    class AccessibleXmp
    {
        public static string ToXmpString(ElementNode element)
        {
            // temporarily just use json
            var json = JsonConvert.SerializeObject(element);
            return json;
        }

        public static ElementNode FromXmpElement(string xmp)
        {
            return JsonConvert.DeserializeObject<ElementNode>(xmp);
        }
    }
}
