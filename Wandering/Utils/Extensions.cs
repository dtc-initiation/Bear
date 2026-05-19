namespace Wandering.Utils;

public static class BitOperations {
    public static int TrailingZeroCount(int flag) {
        int index = 0;
        while (flag > 1) {
            flag >>= 1;
            index++;
        }
        return index;
    }
}