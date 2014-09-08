:: ###############################################################################################################
:: #                                                                                                             #
:: # Set machine-wide enviroment variables used by WiX to test how build will work in APPVEYOR build enviroment. #
:: # For more info check http://www.appveyor.com/docs/environment-variables                                      #
:: #                                                                                                             #
:: ###############################################################################################################
:: #                                                                                                             #
:: # You will have to restart Visual Studio after script execution in order to test installler creation locally  #
:: #                                                                                                             #
:: ###############################################################################################################

SETX CI	"Local test system" /M
SETX APPVEYOR_BUILD_NUMBER "21" /M
SETX APPVEYOR_BUILD_VERSION "1.3.21" /M
SETX APPVEYOR_REPO_NAME "TheAirline/TestRepo" /M
SETX APPVEYOR_REPO_BRANCH "TestBranch" /M
SETX APPVEYOR_REPO_COMMIT "123d45a6" /M
SETX PLATFORM="Debug AnyCPU" /M