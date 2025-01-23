using UnityEngine;
using System.Collections.Generic;

public class MenuCategory : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool selectFirstPageOnEnable = true;
    [SerializeField] private bool disableActivePageOnChange = true;
    [SerializeField] private List<MenuPage> pages = new List<MenuPage>();
    
    
    [Space(10)]
    [CustomAttribute.ReadOnly] public MenuPage currentPage;
    public bool IsAtFirstPage => pages[0].gameObject.activeSelf;

    private void Awake()
    {
        DisableAllPages();
        
    }

    public virtual void OnCategorySelected()
    {
        if (selectFirstPageOnEnable) // Select the first page in the list
        {
            SelectFirstPage();
                
        } else if (currentPage) // Refresh the active page
        {
            SelectPage(currentPage);
        }
    }
    
    public virtual void OnCategoryDeselected()
    {

    }
    
    
    public virtual void OnNavigate(Vector2 input) // Input event
    {
        if (currentPage != null)
        {
            currentPage.OnNavigate(input);
        }
    }

    public virtual void OnToggleMenu() // Input event
    {
        if (currentPage == null) return;
            
        if (currentPage != pages[0])
        {
            SelectFirstPage();
        }

    }
    


#region Pages Handler //-------------------------------------------------------------

    protected virtual void SelectPage(MenuPage page)
    {
        if (!page || !pages.Contains(page)) return; // Return if the page is null or not in the list


        // Deactivate the last page
        if (currentPage)
        {
            currentPage.OnPageDeselected();
            currentPage.gameObject.SetActive(!disableActivePageOnChange);
        }

        
        // Activate the new page
        currentPage = page;
        page.gameObject.SetActive(true);
        page.OnPageSelected();
    }

    protected virtual void SelectFirstPage()
    {
        if (pages.Count >= 0)
        {
            SelectPage(pages[0]);
        }
    }
    
    protected virtual void DisableAllPages()
    {
        foreach (MenuPage page in pages)
        {
            page.gameObject.SetActive(false);
        }
        currentPage = null;
    }
    
    
    protected virtual void DisableNonActivePagesSelectables()
    {
        foreach (MenuPage page in pages)
        {
            if (currentPage == page) continue; // skip the current page
            page.DisableAllSelectables();
        }
    }
    

#endregion Pages Handler //-------------------------------------------------------------




#if UNITY_EDITOR
private void OnValidate()
{
    // Find and add all pages in the category
        
    foreach (Transform child in transform)
    {
        if (!child.TryGetComponent(out MenuPage category)) continue;
        if (!pages.Contains(category))
        {
            pages.Add(category);
        }
    }
    pages.RemoveAll(page => page == null);
}
#endif
    
    
}