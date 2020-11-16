# Changelog
All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this package adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.1] - 2020-07-24
### Fixed
- Rich text hyperlinks: supporting only hyperlinks to Unity's websites. The user needs to be logged in in order for the hyperlinks to work.
- Next button was not disabled on a new tutorial page when the previous page was auto-completed.

## [1.0.0] - 2020-07-16
### Fixed
* Scene view camera's state is restored properly when exiting a tutorial after an assembly reload.
* If clicking a Switch Tutorial button while having unmodified changes in the scene, and choosing to cancel, do not return to the tutorial selection page, instead keep the current tutorial page.
* Prevent showing of Close Tutorials dialog when Play Mode is changing
* Fix tutorial window layouts not functioning after a tutorial project is restarted.
* Hide the instruction title element when it has no content.
* Fixed loading the tutorial scene twice when starting a tutorial, improving the tutorial start-up time.

## [0.5.1] - 2020-07-06
### Added
- `ArbitraryCriterion`: allows tutorial author to specify arbitrary completion criterion.
- "Force default Inspector" checkmark state in `TutorialPageEditor` is now remembered when you click on another asset.
- `TutorialPage` has now `OnBeforePageShown` and `OnAfterPageShown` events to which tutorial author can subscribe to in order perform custom code.

### Changed
- The package is now known as Tutorial Framework instead of Interactive Tutorial Core.

### Fixed
- `BuildStartedCriterion` is evaluated properly also in cases where a build is not started from default Build Settings window.
- Null reference expcetion when trying to view `TutorialPage` without paragraphs in the Inspector.

## [0.5.0] - 2020-06-23
### Added
- `TutorialWelcomePage`/`TutorialModalWindow`:
  - can be authored at real-time,
  - added a simple `TutorialWelcomePageEditor` with "Show Welcome Dialog" functionality,
  - rich text support, and
  - fully configurable button row.
- `LocalizableTextArea` support for `LocalizableString`s, similar to `TextArea` for `string`s.
- Allow specifying `InitialCameraSettings` in `TutorialProjectSettings`.

### Changed
- Masking is not assumed and enforced for every tutorial page, allowing to mix masked and unmasked pages in the same tutorial.
- **Breaking change**: `TutorialWelcomePage` asset refactored without backward-compatibility.
- `TutorialProjectSettings`:
  - the welcome page for the project needs to be set explicitly,
  - `StartupTutorial` is not started automatically, and
  - `UseLegacyStartupBehavior` false by default.
- Authoring: single linebreaks rendered as expected instead of new paragraphs.
- Refactored and combined all the styles, removed `WelcomeDialog.uss`.
- Updated the style of the welcome dialog.
- Improvement: `SceneViewCameraSettingsDrawer` shows rotation as Euler angles instead of raw Quaternion components.
- Improvement: Save and restore SceneView's state (i.e. camera's state) when entering and exiting tutorials.
- Dependencies: update Editor Coroutines to 1.0.0.

### Fixed
- Single linebreaks now make a linebreak and two linebreaks make a paragraph.
- `TutorialModalWindow`: Fixed hiding of `HeaderContainer` if none/null image set.
- Instead of modifying the original window layout files in the project, a working copy is created and modified.
- Fixed null reference exceptions when starting a Tutorial which has no pages.

## [0.4.0] - 2020-06-02
###  Changed
- Refactor `TutorialWindow` to use UI Toolkit instead of IMGUI.
- **Breaking change**: `TutorialContainer` and `TutorialParagraph` assets refactored without backward-compatibility.
- Analytics: using `EditorAnalytics` instead of `UsabilityAnalytics` as the API for all events.
- `TutorialPage`: instead of arbitrary set of `TutorialParagraph`s, the page has a fixed set of fields. `TutorialParagraph` will be deprecated in the near future.
### Added
- `TutorialPageEditor`: a new simplified authoring view.
- Preliminary localization support.
- Analytics: added an event for external reference (e.g. link cards) impressions

## [0.3.0] - 2020-03-25
### Changed
 - Raised the required Unity version to 2019.3.
 - Removed flexible spaces surrounding a video paragraph.
### Fixed
 - UI flickering on macOS and Unity 2019.3 when a tutorial page had a video.

## [0.2.3] - 2020-02-26
### Added
 - Analytics event for clicking external references/URLs.

## [0.2.2] - 2020-02-14
### Added
 - Support for authorized URLs (Unity Connect auto-login).
 
## [0.2.1] - 2019-11-11
### Fixed
 - Do not show the Welcome dialog and load the IET window layout every time an IET project is started.
 - Fixed IET initialization when a Microgame is loaded from the Asset Store.

### Changed
 - Do not clear the description of a tutorial card when a tutorial is marked as completed.
 
## [0.2.0] - 2019-10-21
### Changed
 - `Readme` class renamed to more suitable `TutorialContainer`.
 
## [0.1.18] - 2019-10-21
### Changed
 - New single-panel approach, Readme and Tutorials are shown in the same window which is always visible.
 - Ability to save the Project window's state for the end-user when saving layouts for tutorials.
 - `Readme` class moved into `Unity.InteractiveTutorials` namespace.

## [0.1.17] - 2019-07-19
### Changed
 - Updated UI styles.

## [0.1.16] - 2019-05-15
### Changed
 - Updated warning message when the user is about to exit the tutorial.

## [0.1.15] - 2019-03-04
### Added
 - Adding the ability to unmask elements based on the name of the GUIStyle used to draw them.
 - Warning message when the user is about to exit the tutorial.

### Changed
 - When clicking on *Help > Template Walkthroughs* if the inspector window is not visible, the Inspector window will be shown

## [0.1.14] - 2019-02-12
### Fixed
 - If the user opens an Undocked window, that is not part of the tutorial, the window tabs are unmasked, so they can close or move the window.
 - Improved compatibility with old content.

## [0.1.13] - 2019-02-04
### Added
- Add support for specifying alternate EditorWindow types when configuring unmasked views
- Expand unmask region to include foldout arrow when unmasking property that is collapsed

### Removed
- Remove "Couldn't find a readme" message when there is no Readme asset in project

## [0.1.12] - 2019-01-24
### Fixed
- Fix editor entering and exiting play mode on project load
- Fix unmasked property unmasking entire window when ancestor property is collapsed

## [0.1.11] - 2019-01-17
### Fixed
- Fix 2019.1 compilation errors
- Improve invalid ScriptableObject reference workaround to always exit play mode after project load

## [0.1.10] - 2019-01-11
### Fixed
- Fix invalid CHANGELOG formatting.

## [0.1.9] - 2019-01-11
### Changed
- SceneViewCameraMovedCriterion will also complete if the user changes the camera orientation.

### Fixed
- Added work around for issue where tutorial is not loaded initial project load

## [0.1.8] - 2018-12-11
### Fixed
- Fixed build script

## [0.1.7] - 2018-12-10
### Fixed
- Fixed authoring of scene object references.
### Removed
- Remove *Window > Tutorials* menu item.

## [0.1.6] - 2018-12-06
### Fixed
- Fix AudioClip import errors.
- Fix compilation errors at build time due to incorrectly configured Assembly Definition asset.
- Fix inconsistent line endings.
- Fix CS0649 warnings.
- Fix *Help > Template Walkthroughs* menu item not finding Readme asset.

## [0.1.5] - 2018-12-04
### Fixed
- Fixed ReflectionTypeLoadException when inspecting TutorialPage
- Fixed GUI layout errors when starting tutorial from Readme asset

## [0.1.4] - 2018-12-03
### Added
- Integrated the readme asset with the Tutorials
- Ability to have more than a single Tutorial in a project
- Propper flow for users to go into and out of a tutorial
- Ability to add Images, Video to a tutorial
- New Color type added to PropertyModificaitonCriterion
- PropertyModificationCriterion has a new mode where it will complete if the user changes a property to a different value than initial
- Added option to the masking system to prevent interactions to the unmasked area
- New Criterions: FrameSelectedCriterion, MaterialPropertyChanged, ActiveToolCriterion, SceneCameraViewMovedCritertion
- Ability for Tutorials to reference each other
- "Home/Skip" button have 2 modes. Legacy will open the Hub, and CloseWindow will close the Tutorial window
- Ability to choose the name of the Tutorial Window

### Fixed
- Updated usages of obsolete APIs

### Changed
- Initial version of the in editor tutorial framework as a package.
- Contained the use of internals to a single folder.
