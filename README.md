# Dependencies
This package requires the [R3](https://github.com/Cysharp/R3) library to be present in your project.

# Views

## `BaseView<TViewModel>`
The `BaseView<TViewModel>` class is a cornerstone of this package. It is the main class responsible for binding the View with the ViewModel.

1. The `Initialize` method is used to assign the current ViewModel instance to the view.
2. Dynamic replacement of the ViewModel is supported:
    1. All subscriptions related to the previous ViewModel are automatically removed.
    2. Subscriptions and bindings for the new ViewModel are created anew.

## Creating your own View class

To create your own view, inherit from the `BaseView<TViewModel>` class, specifying your desired ViewModel type as the generic parameter. Then, override the following methods to customize behavior:

1. **`InitializeChildren`**  
   Override this method to initialize any inner (child) views. Inside it, call the `Initialize` method of your child views, passing their corresponding ViewModels.

2. **`BindViewModel`**  
   Override this method to bind your ViewModel's properties to UI elements.  
   To safely subscribe to observables and ensure automatic cleanup, **always use the protected helper method `Bind`** from the base class.  
   *Manual subscriptions must be disposed of manually and are not recommended — using `Bind` prevents memory leaks.*

### Example

```csharp
public class ProfileView : BaseView<ProfileViewModel>
{
    [SerializeField] private UserAvatarView _userAvatarView;
    [SerializeField] private TextMeshProUGUI _usernameLabel;

    protected override void InitializeChildren(ProfileViewModel viewModel)
    {
        base.InitializeChildren(viewModel);
        // Initialize child view with its own ViewModel
        _userAvatarView.Initialize(viewModel.AvatarViewModel);
    }

    protected override void BindViewModel(ProfileViewModel viewModel)
    {
        base.BindViewModel(viewModel);
        // Bind ViewModel property to UI label
        Bind(viewModel.Username, name => _usernameLabel.text = name);
    }
}
```

## CodeGen
Work in progress