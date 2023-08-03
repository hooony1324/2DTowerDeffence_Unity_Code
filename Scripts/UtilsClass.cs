using UnityEngine;

namespace KSH.Utils
{
    public static class UtilsClass
    {
        public static int[] GetNonOverlappingRandomNumbers(int start, int end, int n)
        {
            int size = end - start;

            if (n > size)
            {
                Debug.Log("invalid size,");
                return null;
            }

            int[] result = new int[n];
            int[] seeds = new int[size];

            // initialize seed numbers
            for (int i = 0; i < size; ++i)
            {
                seeds[i] = start++;
            }

            // selecting algorithm
            int indexCount = size;
            for (int i = 0; i < n; ++i)
            {
                int randomIndex = Random.Range(0, indexCount);

                result[i] = seeds[randomIndex];
                seeds[randomIndex] = seeds[indexCount - 1];

                indexCount--;
            }

            return result;
        }

        // Create Text in the world
        public static TextMesh CreateWorldText(string text, Transform parent = null, Vector3 localPosition = default(Vector3), int fontSize = 40,
            Color color = default(Color), TextAnchor textAnchor = TextAnchor.UpperLeft, TextAlignment textAlignment = TextAlignment.Left, int sortingOrder = 0)
        {
            if (color == null)
            {
                color = Color.white;
            }

            return CreateWorldText(parent, text, localPosition, fontSize, (Color)color, textAnchor, textAlignment, sortingOrder);
        }

        // Create Text in the world
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor,
            TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;

            return textMesh;
        }

        // Get Mouse Position
        public static Vector3 GetMouseWorldPosition()
        {
            Vector3 vec = GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
            vec.z = 0f;
            return vec;
        }
        public static Vector3 GetMouseWorldPositionWithZ()
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, Camera.main);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Camera worldCamera)
        {
            return GetMouseWorldPositionWithZ(Input.mousePosition, worldCamera);
        }
        public static Vector3 GetMouseWorldPositionWithZ(Vector3 screenPosition, Camera worldCamera)
        {
            Vector3 worldPosition = worldCamera.ScreenToWorldPoint(screenPosition);
            return worldPosition;
        }
    }

}

