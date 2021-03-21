using Oqtane.Shared.Enums;
using Oqtane.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.ChatHubs.Repository
{
    public static class ChatHubRepositoryExtensionMethods
    {

        public static IQueryable<ChatHubRoom> Enabled(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(r => r.Status == ChatHubRoomStatus.Enabled.ToString());
        }

        public static IQueryable<ChatHubConnection> Active(this IQueryable<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active));
        }

        public static IEnumerable<ChatHubConnection> Active(this ICollection<ChatHubConnection> connections)
        {
            return connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active));
        }

        public static bool Online(this ChatHubUser user)
        {
            return user.Connections.Where(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)).Any();
        }

        public static IQueryable<ChatHubUser> Online(this IQueryable<ChatHubUser> users)
        {
            return users.Where(user => user.Connections.Any(c => c.Status == Enum.GetName(typeof(ChatHubConnectionStatus), ChatHubConnectionStatus.Active)));
        }

        public static IQueryable<ChatHubRoom> Public(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(room => room.Type == ChatHubRoomType.Public.ToString());
        }

        public static IQueryable<ChatHubRoom> Protected(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(room => room.Type == ChatHubRoomType.Protected.ToString());
        }

        public static IQueryable<ChatHubRoom> Private(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(room => room.Type == ChatHubRoomType.Private.ToString());
        }

        public static IQueryable<ChatHubRoom> OneVsOne(this IQueryable<ChatHubRoom> rooms)
        {
            return rooms.Where(room => room.Type == ChatHubRoomType.OneVsOne.ToString());
        }

        public static bool Public(this ChatHubRoom room)
        {
            return room.Type == ChatHubRoomType.Public.ToString();
        }

        public static bool Protected(this ChatHubRoom room)
        {
            return room.Type == ChatHubRoomType.Protected.ToString();
        }

        public static bool Private(this ChatHubRoom room)
        {
            return room.Type == ChatHubRoomType.Private.ToString();
        }

        public static bool OneVsOne(this ChatHubRoom room)
        {
            return room.Type == ChatHubRoomType.OneVsOne.ToString();
        }

    }
}
