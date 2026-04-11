"use client";

import { useEffect, useRef, useState } from "react";
import { getPurchaseOrderMessages, sendPurchaseOrderMessage } from "@/lib/api/purchaseOrders";
import { getLoggedInUserId } from "@/services/authService";
import { formatDateTime } from "@/lib/utils/formatters";
import { fallback, validation } from "@/lib/messages";
import type { PurchaseOrderMessage } from "@/types/purchaseOrders";
import styles from "./MessageThread.module.css";

type Props = {
  purchaseOrderId: string;
};

export default function MessageThread({ purchaseOrderId }: Props) {
  const [messages, setMessages] = useState<PurchaseOrderMessage[]>([]);
  const [body, setBody] = useState("");
  const [error, setError] = useState("");
  const [sending, setSending] = useState(false);
  const [loading, setLoading] = useState(true);
  const scrollRef = useRef<HTMLDivElement>(null);
  const currentUserId = getLoggedInUserId();

  useEffect(() => {
    let cancelled = false;
    (async () => {
      try {
        const data = await getPurchaseOrderMessages(purchaseOrderId);
        if (!cancelled) setMessages(data);
      } catch {
        if (!cancelled) setError(fallback.messageFetchFailed);
      } finally {
        if (!cancelled) setLoading(false);
      }
    })();
    return () => { cancelled = true; };
  }, [purchaseOrderId]);

  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [messages]);

  const handleSend = async () => {
    const trimmed = body.trim();
    if (!trimmed) {
      setError(validation.required("メッセージ"));
      return;
    }
    if (trimmed.length > 500) {
      setError(validation.maxLength("メッセージ", 500));
      return;
    }

    setError("");
    setSending(true);
    try {
      const newMessage = await sendPurchaseOrderMessage(purchaseOrderId, trimmed);
      setMessages((prev) => [...prev, newMessage]);
      setBody("");
    } catch (e) {
      setError(e instanceof Error ? e.message : fallback.messageSendFailed);
    } finally {
      setSending(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === "Enter" && (e.ctrlKey || e.metaKey)) {
      e.preventDefault();
      handleSend();
    }
  };

  if (loading) {
    return <div className={styles.loading}>メッセージを読み込み中...</div>;
  }

  return (
    <div className={styles.container}>
      <h3 className={styles.title}>メッセージ</h3>

      <div className={styles.messageList} ref={scrollRef}>
        {messages.length === 0 && (
          <p className={styles.empty}>メッセージはまだありません。</p>
        )}
        {messages.map((msg) => {
          const isSelf = msg.sentBy === currentUserId;
          return (
            <div
              key={msg.purchaseOrderMessageId}
              className={`${styles.bubble} ${isSelf ? styles.self : styles.other}`}
            >
              <div className={styles.meta}>
                <span className={styles.sender}>{msg.senderName}</span>
                <span className={styles.time}>{formatDateTime(msg.createdAt)}</span>
              </div>
              <div className={styles.body}>{msg.body}</div>
            </div>
          );
        })}
      </div>

      {error && <p className={styles.error}>{error}</p>}

      <div className={styles.inputArea}>
        <textarea
          className={styles.textarea}
          value={body}
          onChange={(e) => setBody(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="メッセージを入力（Ctrl+Enterで送信）"
          maxLength={500}
          rows={2}
          disabled={sending}
        />
        <button
          className={styles.sendButton}
          onClick={handleSend}
          disabled={sending || !body.trim()}
        >
          {sending ? "送信中..." : "送信"}
        </button>
      </div>
    </div>
  );
}
