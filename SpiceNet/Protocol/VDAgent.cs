namespace SpiceNet.Protocol;

public static partial class VDAgent
{
    public const int VD_AGENT_PROTOCOL = 1;
    public const int VD_AGENT_MAX_DATA_SIZE = 2048;

    public const int VDP_CLIENT_PORT = 1;
    public const int VDP_SERVER_PORT = 2;
    public const int VDP_END_PORT = 3;

    public const int VD_AGENT_MOUSE_STATE = 1;
    public const int VD_AGENT_MONITORS_CONFIG = 2;
    public const int VD_AGENT_REPLY = 3;
    public const int VD_AGENT_CLIPBOARD = 4;
    public const int VD_AGENT_DISPLAY_CONFIG = 5;
    public const int VD_AGENT_ANNOUNCE_CAPABILITIES = 6;
    public const int VD_AGENT_CLIPBOARD_GRAB = 7;
    public const int VD_AGENT_CLIPBOARD_REQUEST = 8;
    public const int VD_AGENT_CLIPBOARD_RELEASE = 9;
    public const int VD_AGENT_FILE_XFER_START = 10;
    public const int VD_AGENT_FILE_XFER_STATUS = 11;
    public const int VD_AGENT_FILE_XFER_DATA = 12;
    public const int VD_AGENT_CLIENT_DISCONNECTED = 13;
    public const int VD_AGENT_MAX_CLIPBOARD = 14;
    public const int VD_AGENT_AUDIO_VOLUME_SYNC = 15;
    public const int VD_AGENT_GRAPHICS_DEVICE_INFO = 16;
    public const int VD_AGENT_END_MESSAGE = 17;

    public const int VD_AGENT_FILE_XFER_STATUS_CAN_SEND_DATA = 0;
    public const int VD_AGENT_FILE_XFER_STATUS_CANCELLED = 1;
    public const int VD_AGENT_FILE_XFER_STATUS_ERROR = 2;
    public const int VD_AGENT_FILE_XFER_STATUS_SUCCESS = 3;
    public const int VD_AGENT_FILE_XFER_STATUS_NOT_ENOUGH_SPACE = 4;
    public const int VD_AGENT_FILE_XFER_STATUS_SESSION_LOCKED = 5;
    public const int VD_AGENT_FILE_XFER_STATUS_VDAGENT_NOT_CONNECTED = 6;
    public const int VD_AGENT_FILE_XFER_STATUS_DISABLED = 7;

    public const int VD_AGENT_FILE_XFER_STATUS_ERROR_GLIB_IO = 0;

    public const int VD_AGENT_CONFIG_MONITORS_FLAG_USE_POS = (1 << 0);
    public const int VD_AGENT_CONFIG_MONITORS_FLAG_PHYSICAL_SIZE = (1 << 1);

    public const int VD_AGENT_DISPLAY_CONFIG_FLAG_DISABLE_WALLPAPER = (1 << 0);
    public const int VD_AGENT_DISPLAY_CONFIG_FLAG_DISABLE_FONT_SMOOTH = (1 << 1);
    public const int VD_AGENT_DISPLAY_CONFIG_FLAG_DISABLE_ANIMATION = (1 << 2);
    public const int VD_AGENT_DISPLAY_CONFIG_FLAG_SET_COLOR_DEPTH = (1 << 3);

    public const int VD_AGENT_SUCCESS = 1;
    public const int VD_AGENT_ERROR = 2;

    public const int VD_AGENT_CLIPBOARD_NONE = 0;
    public const int VD_AGENT_CLIPBOARD_UTF8_TEXT = 1;
    public const int VD_AGENT_CLIPBOARD_IMAGE_PNG = 2;
    public const int VD_AGENT_CLIPBOARD_IMAGE_BMP = 3;
    public const int VD_AGENT_CLIPBOARD_IMAGE_TIFF = 4;
    public const int VD_AGENT_CLIPBOARD_IMAGE_JPG = 5;
    public const int VD_AGENT_CLIPBOARD_FILE_LIST = 6;

    public const int VD_AGENT_CLIPBOARD_SELECTION_CLIPBOARD = 0;
    public const int VD_AGENT_CLIPBOARD_SELECTION_PRIMARY = 1;
    public const int VD_AGENT_CLIPBOARD_SELECTION_SECONDARY = 2;

    public const int VD_AGENT_CAP_MOUSE_STATE = 0;
    public const int VD_AGENT_CAP_MONITORS_CONFIG = 1;
    public const int VD_AGENT_CAP_REPLY = 2;
    public const int VD_AGENT_CAP_CLIPBOARD = 3;
    public const int VD_AGENT_CAP_DISPLAY_CONFIG = 4;
    public const int VD_AGENT_CAP_CLIPBOARD_BY_DEMAND = 5;
    public const int VD_AGENT_CAP_CLIPBOARD_SELECTION = 6;
    public const int VD_AGENT_CAP_SPARSE_MONITORS_CONFIG = 7;
    public const int VD_AGENT_CAP_GUEST_LINEEND_LF = 8;
    public const int VD_AGENT_CAP_GUEST_LINEEND_CRLF = 9;
    public const int VD_AGENT_CAP_MAX_CLIPBOARD = 10;
    public const int VD_AGENT_CAP_AUDIO_VOLUME_SYNC = 11;
    public const int VD_AGENT_CAP_MONITORS_CONFIG_POSITION = 12;
    public const int VD_AGENT_CAP_FILE_XFER_DISABLED = 13;
    public const int VD_AGENT_CAP_FILE_XFER_DETAILED_ERRORS = 14;
    public const int VD_AGENT_CAP_GRAPHICS_DEVICE_INFO = 15;
    public const int VD_AGENT_CAP_CLIPBOARD_NO_RELEASE_ON_REGRAB = 16;
    public const int VD_AGENT_CAP_CLIPBOARD_GRAB_SERIAL = 17;
    public const int VD_AGENT_CAP_MONITORS_PHYSICAL_SIZE = 18;
    public const int VD_AGENT_END_CAP = 19;

}
