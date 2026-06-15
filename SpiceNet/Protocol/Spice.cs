namespace SpiceNet.Protocol;

public static partial class Spice
{
    public const int SPICE_CHANNEL_MAIN = 1;
    public const int SPICE_CHANNEL_DISPLAY = 2;
    public const int SPICE_CHANNEL_INPUTS = 3;
    public const int SPICE_CHANNEL_CURSOR = 4;
    public const int SPICE_CHANNEL_PLAYBACK = 5;
    public const int SPICE_CHANNEL_RECORD = 6;
    public const int SPICE_CHANNEL_TUNNEL = 7;
    public const int SPICE_CHANNEL_SMARTCARD = 8;
    public const int SPICE_CHANNEL_USBREDIR = 9;
    public const int SPICE_CHANNEL_PORT = 10;
    public const int SPICE_CHANNEL_WEBDAV = 11;
    public const int SPICE_END_CHANNEL = 12;

    public const int SPICE_MSG_MIGRATE = 1;
    public const int SPICE_MSG_MIGRATE_DATA = 2;
    public const int SPICE_MSG_SET_ACK = 3;
    public const int SPICE_MSG_PING = 4;
    public const int SPICE_MSG_WAIT_FOR_CHANNELS = 5;
    public const int SPICE_MSG_DISCONNECTING = 6;
    public const int SPICE_MSG_NOTIFY = 7;
    public const int SPICE_MSG_LIST = 8;
    public const int SPICE_MSG_BASE_LAST = 100;

    public const int SPICE_MSGC_ACK_SYNC = 1;
    public const int SPICE_MSGC_ACK = 2;
    public const int SPICE_MSGC_PONG = 3;
    public const int SPICE_MSGC_MIGRATE_FLUSH_MARK = 4;
    public const int SPICE_MSGC_MIGRATE_DATA = 5;
    public const int SPICE_MSGC_DISCONNECTING = 6;

    public const int SPICE_MSG_MAIN_MIGRATE_BEGIN = 101;
    public const int SPICE_MSG_MAIN_MIGRATE_CANCEL = 102;
    public const int SPICE_MSG_MAIN_INIT = 103;
    public const int SPICE_MSG_MAIN_CHANNELS_LIST = 104;
    public const int SPICE_MSG_MAIN_MOUSE_MODE = 105;
    public const int SPICE_MSG_MAIN_MULTI_MEDIA_TIME = 106;
    public const int SPICE_MSG_MAIN_AGENT_CONNECTED = 107;
    public const int SPICE_MSG_MAIN_AGENT_DISCONNECTED = 108;
    public const int SPICE_MSG_MAIN_AGENT_DATA = 109;
    public const int SPICE_MSG_MAIN_AGENT_TOKEN = 110;
    public const int SPICE_MSG_MAIN_MIGRATE_SWITCH_HOST = 111;
    public const int SPICE_MSG_MAIN_MIGRATE_END = 112;
    public const int SPICE_MSG_MAIN_NAME = 113;
    public const int SPICE_MSG_MAIN_UUID = 114;
    public const int SPICE_MSG_MAIN_AGENT_CONNECTED_TOKENS = 115;
    public const int SPICE_MSG_MAIN_MIGRATE_BEGIN_SEAMLESS = 116;
    public const int SPICE_MSG_MAIN_MIGRATE_DST_SEAMLESS_ACK = 117;
    public const int SPICE_MSG_MAIN_MIGRATE_DST_SEAMLESS_NACK = 118;
    public const int SPICE_MSG_END_MAIN = 119;

    public const int SPICE_MSGC_MAIN_CLIENT_INFO = 101;
    public const int SPICE_MSGC_MAIN_MIGRATE_CONNECTED = 102;
    public const int SPICE_MSGC_MAIN_MIGRATE_CONNECT_ERROR = 103;
    public const int SPICE_MSGC_MAIN_ATTACH_CHANNELS = 104;
    public const int SPICE_MSGC_MAIN_MOUSE_MODE_REQUEST = 105;
    public const int SPICE_MSGC_MAIN_AGENT_START = 106;
    public const int SPICE_MSGC_MAIN_AGENT_DATA = 107;
    public const int SPICE_MSGC_MAIN_AGENT_TOKEN = 108;
    public const int SPICE_MSGC_MAIN_MIGRATE_END = 109;
    public const int SPICE_MSGC_MAIN_MIGRATE_DST_DO_SEAMLESS = 110;
    public const int SPICE_MSGC_MAIN_MIGRATE_CONNECTED_SEAMLESS = 111;
    public const int SPICE_MSGC_MAIN_QUALITY_INDICATOR = 112;
    public const int SPICE_MSGC_END_MAIN = 113;

    public const int SPICE_MSG_DISPLAY_MODE = 101;
    public const int SPICE_MSG_DISPLAY_MARK = 102;
    public const int SPICE_MSG_DISPLAY_RESET = 103;
    public const int SPICE_MSG_DISPLAY_COPY_BITS = 104;
    public const int SPICE_MSG_DISPLAY_INVAL_LIST = 105;
    public const int SPICE_MSG_DISPLAY_INVAL_ALL_PIXMAPS = 106;
    public const int SPICE_MSG_DISPLAY_INVAL_PALETTE = 107;
    public const int SPICE_MSG_DISPLAY_INVAL_ALL_PALETTES = 108;
    public const int SPICE_MSG_DISPLAY_STREAM_CREATE = 122;
    public const int SPICE_MSG_DISPLAY_STREAM_DATA = 123;
    public const int SPICE_MSG_DISPLAY_STREAM_CLIP = 124;
    public const int SPICE_MSG_DISPLAY_STREAM_DESTROY = 125;
    public const int SPICE_MSG_DISPLAY_STREAM_DESTROY_ALL = 126;
    public const int SPICE_MSG_DISPLAY_DRAW_FILL = 302;
    public const int SPICE_MSG_DISPLAY_DRAW_OPAQUE = 303;
    public const int SPICE_MSG_DISPLAY_DRAW_COPY = 304;
    public const int SPICE_MSG_DISPLAY_DRAW_BLEND = 305;
    public const int SPICE_MSG_DISPLAY_DRAW_BLACKNESS = 306;
    public const int SPICE_MSG_DISPLAY_DRAW_WHITENESS = 307;
    public const int SPICE_MSG_DISPLAY_DRAW_INVERS = 308;
    public const int SPICE_MSG_DISPLAY_DRAW_ROP3 = 309;
    public const int SPICE_MSG_DISPLAY_DRAW_STROKE = 310;
    public const int SPICE_MSG_DISPLAY_DRAW_TEXT = 311;
    public const int SPICE_MSG_DISPLAY_DRAW_TRANSPARENT = 312;
    public const int SPICE_MSG_DISPLAY_DRAW_ALPHA_BLEND = 313;
    public const int SPICE_MSG_DISPLAY_SURFACE_CREATE = 314;
    public const int SPICE_MSG_DISPLAY_SURFACE_DESTROY = 315;
    public const int SPICE_MSG_DISPLAY_STREAM_DATA_SIZED = 316;
    public const int SPICE_MSG_DISPLAY_MONITORS_CONFIG = 317;
    public const int SPICE_MSG_DISPLAY_DRAW_COMPOSITE = 318;
    public const int SPICE_MSG_DISPLAY_STREAM_ACTIVATE_REPORT = 319;
    public const int SPICE_MSG_DISPLAY_GL_SCANOUT_UNIX = 320;
    public const int SPICE_MSG_DISPLAY_GL_DRAW = 321;
    public const int SPICE_MSG_DISPLAY_QUALITY_INDICATOR = 322;
    public const int SPICE_MSG_DISPLAY_GL_SCANOUT2_UNIX = 323;
    public const int SPICE_MSG_END_DISPLAY = 324;

    public const int SPICE_MSGC_DISPLAY_INIT = 101;
    public const int SPICE_MSGC_DISPLAY_STREAM_REPORT = 102;
    public const int SPICE_MSGC_DISPLAY_PREFERRED_COMPRESSION = 103;
    public const int SPICE_MSGC_DISPLAY_GL_DRAW_DONE = 104;
    public const int SPICE_MSGC_DISPLAY_PREFERRED_VIDEO_CODEC_TYPE = 105;
    public const int SPICE_MSGC_END_DISPLAY = 106;

    public const int SPICE_MSG_INPUTS_INIT = 101;
    public const int SPICE_MSG_INPUTS_KEY_MODIFIERS = 102;
    public const int SPICE_MSG_INPUTS_MOUSE_MOTION_ACK = 111;
    public const int SPICE_MSG_END_INPUTS = 112;

    public const int SPICE_MSGC_INPUTS_KEY_DOWN = 101;
    public const int SPICE_MSGC_INPUTS_KEY_UP = 102;
    public const int SPICE_MSGC_INPUTS_KEY_MODIFIERS = 103;
    public const int SPICE_MSGC_INPUTS_KEY_SCANCODE = 104;
    public const int SPICE_MSGC_INPUTS_MOUSE_MOTION = 111;
    public const int SPICE_MSGC_INPUTS_MOUSE_POSITION = 112;
    public const int SPICE_MSGC_INPUTS_MOUSE_PRESS = 113;
    public const int SPICE_MSGC_INPUTS_MOUSE_RELEASE = 114;
    public const int SPICE_MSGC_END_INPUTS = 115;

    public const int SPICE_MSG_CURSOR_INIT = 101;
    public const int SPICE_MSG_CURSOR_RESET = 102;
    public const int SPICE_MSG_CURSOR_SET = 103;
    public const int SPICE_MSG_CURSOR_MOVE = 104;
    public const int SPICE_MSG_CURSOR_HIDE = 105;
    public const int SPICE_MSG_CURSOR_TRAIL = 106;
    public const int SPICE_MSG_CURSOR_INVAL_ONE = 107;
    public const int SPICE_MSG_CURSOR_INVAL_ALL = 108;
    public const int SPICE_MSG_END_CURSOR = 109;

    public const int SPICE_MSG_PLAYBACK_DATA = 101;
    public const int SPICE_MSG_PLAYBACK_MODE = 102;
    public const int SPICE_MSG_PLAYBACK_START = 103;
    public const int SPICE_MSG_PLAYBACK_STOP = 104;
    public const int SPICE_MSG_PLAYBACK_VOLUME = 105;
    public const int SPICE_MSG_PLAYBACK_MUTE = 106;
    public const int SPICE_MSG_PLAYBACK_LATENCY = 107;
    public const int SPICE_MSG_END_PLAYBACK = 108;

    public const int SPICE_MSG_RECORD_START = 101;
    public const int SPICE_MSG_RECORD_STOP = 102;
    public const int SPICE_MSG_RECORD_VOLUME = 103;
    public const int SPICE_MSG_RECORD_MUTE = 104;
    public const int SPICE_MSG_END_RECORD = 105;

    public const int SPICE_MSGC_RECORD_DATA = 101;
    public const int SPICE_MSGC_RECORD_MODE = 102;
    public const int SPICE_MSGC_RECORD_START_MARK = 103;
    public const int SPICE_MSGC_END_RECORD = 104;

    public const int SPICE_MSG_SMARTCARD_DATA = 101;
    public const int SPICE_MSG_END_SMARTCARD = 102;

    public const int SPICE_MSGC_SMARTCARD_DATA = 101;
    public const int SPICE_MSGC_SMARTCARD_HEADER = 101;
    public const int SPICE_MSGC_SMARTCARD_ERROR = 101;
    public const int SPICE_MSGC_SMARTCARD_ATR = 101;
    public const int SPICE_MSGC_SMARTCARD_READER_ADD = 101;
    public const int SPICE_MSGC_END_SMARTCARD = 102;

    public const int SPICE_MSG_SPICEVMC_DATA = 101;
    public const int SPICE_MSG_SPICEVMC_COMPRESSED_DATA = 102;
    public const int SPICE_MSG_END_SPICEVMC = 103;

    public const int SPICE_MSGC_SPICEVMC_DATA = 101;
    public const int SPICE_MSGC_SPICEVMC_COMPRESSED_DATA = 102;
    public const int SPICE_MSGC_END_SPICEVMC = 103;

    public const int SPICE_MSG_PORT_INIT = 201;
    public const int SPICE_MSG_PORT_EVENT = 202;
    public const int SPICE_MSG_END_PORT = 203;

    public const int SPICE_MSGC_PORT_EVENT = 201;
    public const int SPICE_MSGC_END_PORT = 202;

    public const int SPICE_MAGIC = 0x51444552;
    public const int SPICE_VERSION_MAJOR = 2;
    public const int SPICE_VERSION_MINOR = 2;

    public const int SPICE_COMMON_CAP_PROTOCOL_AUTH_SELECTION = 0;
    public const int SPICE_COMMON_CAP_AUTH_SPICE = 1;
    public const int SPICE_COMMON_CAP_AUTH_SASL = 2;
    public const int SPICE_COMMON_CAP_MINI_HEADER = 3;

    public const int SPICE_PLAYBACK_CAP_CELT_0_5_1 = 0;
    public const int SPICE_PLAYBACK_CAP_VOLUME = 1;
    public const int SPICE_PLAYBACK_CAP_LATENCY = 2;
    public const int SPICE_PLAYBACK_CAP_OPUS = 3;

    public const int SPICE_RECORD_CAP_CELT_0_5_1 = 0;
    public const int SPICE_RECORD_CAP_VOLUME = 1;
    public const int SPICE_RECORD_CAP_OPUS = 2;

    public const int SPICE_MAIN_CAP_SEMI_SEAMLESS_MIGRATE = 0;
    public const int SPICE_MAIN_CAP_NAME_AND_UUID = 1;
    public const int SPICE_MAIN_CAP_AGENT_CONNECTED_TOKENS = 2;
    public const int SPICE_MAIN_CAP_SEAMLESS_MIGRATE = 3;

    public const int SPICE_DISPLAY_CAP_SIZED_STREAM = 0;
    public const int SPICE_DISPLAY_CAP_MONITORS_CONFIG = 1;
    public const int SPICE_DISPLAY_CAP_COMPOSITE = 2;
    public const int SPICE_DISPLAY_CAP_A8_SURFACE = 3;
    public const int SPICE_DISPLAY_CAP_STREAM_REPORT = 4;
    public const int SPICE_DISPLAY_CAP_LZ4_COMPRESSION = 5;
    public const int SPICE_DISPLAY_CAP_PREF_COMPRESSION = 6;
    public const int SPICE_DISPLAY_CAP_GL_SCANOUT = 7;
    public const int SPICE_DISPLAY_CAP_MULTI_CODEC = 8;
    public const int SPICE_DISPLAY_CAP_CODEC_MJPEG = 9;
    public const int SPICE_DISPLAY_CAP_CODEC_VP8 = 10;
    public const int SPICE_DISPLAY_CAP_CODEC_H264 = 11;
    public const int SPICE_DISPLAY_CAP_PREF_VIDEO_CODEC_TYPE = 12;
    public const int SPICE_DISPLAY_CAP_CODEC_VP9 = 13;
    public const int SPICE_DISPLAY_CAP_CODEC_H265 = 14;
    public const int SPICE_DISPLAY_CAP_GL_SCANOUT2 = 15;

    public const int SPICE_INPUTS_CAP_KEY_SCANCODE = 0;

    public const int SPICE_SPICEVMC_CAP_DATA_COMPRESS_LZ4 = 0;

    public const int SPICE_PORT_EVENT_OPENED = 0;
    public const int SPICE_PORT_EVENT_CLOSED = 1;
    public const int SPICE_PORT_EVENT_BREAK = 2;

    public const int SPICE_INPUT_MOTION_ACK_BUNCH = 4;

    public const int SPICE_MOUSE_MODE_SERVER = 1 << 0;
    public const int SPICE_MOUSE_MODE_CLIENT = 1 << 1;
    public const int SPICE_MOUSE_MODE_MASK = 0x3;

    public const int SPICE_SURFACE_FLAGS_PRIMARY = 1 << 0;

    public const int SPICE_IMAGE_FLAGS_CACHE_ME = 1 << 0;
}
