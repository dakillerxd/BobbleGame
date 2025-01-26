using UnityEngine;
using System.Collections.Generic;
using CustomAttribute;

public class MenuController : MonoBehaviour
{
    [Header("Settings")] 
    [SerializeField] private bool selectFirstCategoryOnEnable = true;
    [SerializeField] private bool disableActiveCategoryOnChange = true;
    [SerializeField] private List<MenuCategory> menuCategories = new List<MenuCategory>();

    [Header("References")]
    [SerializeField] private SOInputReader inputReader;
    [SerializeField] private AudioSource audioSource;
    

    [Space(10)]
    [ReadOnly] public MenuCategory currentCategory;
    public AudioSource AudioSource => audioSource;
    
    protected virtual void Awake()
    {
        DisableAllCategories();
    }
    
    private void OnEnable()
    {
        if (selectFirstCategoryOnEnable)
        {
            SelectFirstCategory();
        }
        
        inputReader.NavigateEvent += OnNavigate;
        inputReader.ToggleMenuEvent += OnToggleMenu;
    }

    private void OnDisable()
    {
        inputReader.NavigateEvent -= OnNavigate;
        inputReader.ToggleMenuEvent -= OnToggleMenu;
    }
    
    private void OnNavigate(Vector2 context) // Input event
    {

        if (currentCategory) 
        {
            currentCategory.OnNavigate(context);
        }
    }
    
    private void OnToggleMenu() // Input event
    {
        if (!currentCategory) return;
        
        
        if (!currentCategory.IsAtFirstPage) // if the first page is not the one that is selected pass on the event
        {
            currentCategory.OnToggleMenu();
                
        } else if (currentCategory != menuCategories[0]) { // Else go to the first menu
                
            SelectFirstCategory();
        }

    }
    

#region Category Handle //-------------------------------------------------------------

    protected virtual void SelectCategory(MenuCategory category)
    {
        if (category == null || !menuCategories.Contains(category)) return;

        if (currentCategory) // Disable active category
        {
            currentCategory.OnCategoryDeselected();
            currentCategory.gameObject.SetActive(!disableActiveCategoryOnChange);
        }

        currentCategory = category;
        category.gameObject.SetActive(true);
        category.OnCategorySelected();
    }

    protected virtual void SelectFirstCategory()
    {
        if (menuCategories.Count >= 0 && menuCategories[0])
        {
            SelectCategory(menuCategories[0]);
        }
    }
        
    protected virtual void DisableAllCategories()
    {
        foreach (MenuCategory category in menuCategories)
        {
            category.gameObject.SetActive(false);
        }
        currentCategory = null;
    }


#endregion Category Handle //-------------------------------------------------------------


#if UNITY_EDITOR
private void OnValidate()
{
    // Find and add all child categories to list
    foreach (Transform child in transform)
    {
        if (!child.TryGetComponent(out MenuCategory category)) continue;
        if (!menuCategories.Contains(category))
        {
            menuCategories.Add(category);
        }
    }
    menuCategories.RemoveAll(menu => menu == null);
}
#endif


}
