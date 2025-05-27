using UnityEngine;

public class ScrollAreaExpander : MonoBehaviour
{
    [SerializeField] private GameObject item_ui_prefab;
    [SerializeField] private float item_padding = 0f;
    [SerializeField] private int item_count = 0;
    private GameObject scroll_area;
    private RectTransform content_area;
    private float item_height;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // lookup cargo area for this object
        scroll_area = this.transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
        // calculate the height of the cargo ui prefab
        RectTransform cargo_item = item_ui_prefab.GetComponent<RectTransform>();
        content_area = scroll_area.GetComponent<RectTransform>();
        Vector3[] v = new Vector3[4];
        cargo_item.GetLocalCorners(v);
        item_height = v[1].y - v[0].y + item_padding;
        // update scroll area size
        content_area.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical,
            item_height * item_count);
    }
}
